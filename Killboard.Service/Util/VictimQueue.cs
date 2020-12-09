using Killboard.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace Killboard.Service.Util
{
    public class VictimQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly ConcurrentQueue<victims> _objs = new ConcurrentQueue<victims>();

        private readonly ILogger<VictimQueue> _logger;
        private readonly DbContextOptions<KillboardContext> _dbContextOptions;
        public VictimQueue(ILogger<VictimQueue> logger, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>()
                .UseSqlServer(configuration["Killboard:Sql"]).Options;
        }

        public void Enqueue(victims obj)
        {
            lock (_objs)
            {
                _objs.Enqueue(obj);
                
                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(int killmailId, long charId) => _objs.Any(a => a.killmail_id == killmailId && a.char_id == charId);

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                victims item;
                lock (_objs)
                {
                    if (_objs.Count == 0)
                    {
                        _delegateQueuedOrRunning = false;
                        break;
                    }

                    if(!_objs.TryDequeue(out item)) continue;
                }

                try
                {
                    _logger.LogInformation($"Processing Victim for Character ID {item.char_id} and Killmail ID {item.killmail_id}");

                    AddObjectToDatabase(item);
                }
                catch (Exception ex) when (ex is DbUpdateException || ex is DbUpdateConcurrencyException)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Failed inserting Victim for Character {CharacterID} and Killmail ID: {KillmailID} - Possible Duplicate Insert", item.char_id, item.killmail_id);
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Fatal Exception inserting Victim for Character {CharacterID} and Killmail ID: {KillmailID}", item.char_id, item.killmail_id);
                }
            }
        }

        private void AddObjectToDatabase(victims obj)
        {
            using var ctx = new KillboardContext(_dbContextOptions);
            
            if (ctx.victims.Any(k => k.killmail_id == obj.killmail_id && k.char_id == obj.char_id)) return;

            ctx.victims.Add(obj);
            ctx.SaveChanges();
        }
    }
}
