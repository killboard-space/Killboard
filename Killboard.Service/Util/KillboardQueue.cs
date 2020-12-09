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
    public class KillboardQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly ConcurrentQueue<killmails> _objs = new ConcurrentQueue<killmails>();

        private readonly ILogger<KillboardQueue> _logger;
        private readonly DbContextOptions<KillboardContext> _dbContextOptions;

        public KillboardQueue(ILogger<KillboardQueue> logger, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>().UseSqlServer(configuration["Killboard:Sql"]).Options;
        }

        public void Enqueue(killmails obj)
        {
            lock (_objs)
            {
                _objs.Enqueue(obj);
                
                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(int killmailId) => _objs.Any(k => k.killmail_id == killmailId);

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                killmails item;
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
                    _logger.LogInformation($"Processing Killmail ID/Hash for Killmail ID {item.killmail_id}");

                    AddObjectToDatabase(item);
                }
                catch (DbUpdateException ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Failed inserting Killmail ID/Hash for Killmail ID: {KillmailID} - Possible Duplicate Insert", item.killmail_id);
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Fatal Exception inserting Killmail ID/Hash for Killmail ID: {KillmailID}", item.killmail_id);
                }
            }
        }

        private void AddObjectToDatabase(killmails obj)
        {
            using var ctx = new KillboardContext(_dbContextOptions);
            
            if (ctx.killmails.Any(k => k.killmail_id == obj.killmail_id)) return;

            ctx.killmails.Add(obj);
            ctx.SaveChanges();
        }
    }
}
