using Killboard.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Killboard.Service.Util
{
    public class AttackerQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly Queue<attackers> _objs = new Queue<attackers>();

        private readonly ILogger<AttackerQueue> _logger;

        public AttackerQueue(ILogger<AttackerQueue> logger)
        {
            _logger = logger;
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

        public bool IsInQueue(int charId, int killmailId) => _objs.Any(a => a.char_id == charId && a.killmail_id == killmailId);

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

                    item = _objs.Dequeue();
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

        private static void AddObjectToDatabase(attackers obj)
        {
            using var ctx = new KillboardContext();
            
            if (ctx.attackers.Any(k => k.char_id == obj.char_id && k.killmail_id == obj.killmail_id)) return;

            ctx.attackers.Add(obj);
            ctx.SaveChanges();
        }
    }
}
