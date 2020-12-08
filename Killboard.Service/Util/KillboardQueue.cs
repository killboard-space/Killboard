using Killboard.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Killboard.Service.Util
{
    public class KillboardQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly Queue<killmails> _objs = new Queue<killmails>();

        private readonly ILogger<KillboardQueue> _logger;

        public KillboardQueue(ILogger<KillboardQueue> logger)
        {
            _logger = logger;
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

                    item = _objs.Dequeue();
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

        private static void AddObjectToDatabase(killmails obj)
        {
            using var ctx = new KillboardContext();
            
            if (ctx.killmails.Any(k => k.killmail_id == obj.killmail_id)) return;

            ctx.killmails.Add(obj);
            ctx.SaveChanges();
        }
    }
}
