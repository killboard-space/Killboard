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
    public class AttackerQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly ConcurrentQueue<attackers> _objs = new ConcurrentQueue<attackers>();

        private readonly ILogger<AttackerQueue> _logger;
        private readonly DbContextOptions<KillboardContext> _dbContextOptions;

        public AttackerQueue(ILogger<AttackerQueue> logger, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>()
                .UseSqlServer(configuration["Killboard:Sql"]).Options;
        }

        public void Enqueue(attackers obj)
        {
            lock (_objs)
            {
                _objs.Enqueue(obj);
                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(int killmailId, long? charId = null, int? corpId = null, int? allianceId = null) =>
            _objs.Any(a => a.killmail_id == killmailId && (charId.HasValue && a.char_id == charId||
                                                           corpId.HasValue && a.corporation_id == corpId ||
                                                           allianceId.HasValue && a.alliance_id == allianceId));

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                attackers item;
                lock (_objs)
                {
                    if (_objs.Count == 0)
                    {
                        _delegateQueuedOrRunning = false;
                        break;
                    }

                    if (!_objs.TryDequeue(out item)) continue;
                }

                try
                {
                    _logger.LogInformation($"Processing Attacker for Character ID {item.char_id} & Killmail ID {item.killmail_id}");

                    AddObjectToDatabase(item);
                }
                catch (DbUpdateException ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Failed inserting Attacker for Character: {CharacterID} and Killmail ID: {KillmailID} - Possible Duplicate Insert", item.char_id,item.killmail_id);
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Fatal Exception inserting Attacker for Character: {CharacterID} and Killmail ID: {KillmailID}", item.char_id, item.killmail_id);
                }
            }
        }

        private void AddObjectToDatabase(attackers obj)
        {
            using var ctx = new KillboardContext(_dbContextOptions);
            
            if (ctx.attackers.Any(k => k.char_id == obj.char_id && k.killmail_id == obj.killmail_id)) return;

            ctx.attackers.Add(obj);
            ctx.SaveChanges();
        }
    }
}
