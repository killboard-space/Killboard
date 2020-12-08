using Killboard.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Killboard.Service.Util
{
    public class AllianceQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly Queue<alliances> _objs = new Queue<alliances>();

        private readonly ILogger<AllianceQueue> _logger;

        public AllianceQueue(ILogger<AllianceQueue> logger)
        {
            _logger = logger;
        }

        public void Enqueue(alliances obj)
        {
            lock (_objs)
            {
                _objs.Enqueue(obj);
                
                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(int allianceId) => _objs.Any(k => k.alliance_id == allianceId);

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                alliances item;
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
                    _logger.LogInformation($"Processing Alliance for Alliance ID {item.alliance_id}");

                    AddObjectToDatabase(item);
                }
                catch (DbUpdateException ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Failed inserting Alliance for Alliance ID: {AllianceID} - Possible Duplicate Insert", item.alliance_id);
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Fatal Exception inserting Alliance for Alliance ID: {AllianceID}", item.alliance_id);
                }
            }
        }

        private static void AddObjectToDatabase(alliances obj)
        {
            using var ctx = new KillboardContext();
            
            if (ctx.alliances.Any(k => k.alliance_id == obj.alliance_id)) return;

            ctx.alliances.Add(obj);
            ctx.SaveChanges();
        }
    }
}
