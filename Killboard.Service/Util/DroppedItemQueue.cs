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
    public class DroppedItemQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly ConcurrentQueue<dropped_items> _objs = new ConcurrentQueue<dropped_items>();

        private readonly ILogger<DroppedItemQueue> _logger;
        private readonly DbContextOptions<KillboardContext> _dbContextOptions;

        public DroppedItemQueue(ILogger<DroppedItemQueue> logger, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>()
                .UseSqlServer(configuration["Killboard:Sql"]).Options;
        }

        public void Enqueue(dropped_items obj)
        {
            lock (_objs)
            {
                _objs.Enqueue(obj);
                
                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(int killmailId, int itemTypeId, int? quantDropped = null, int? quantDestroy = null) =>
            _objs.Any(a => a.killmail_id == killmailId && a.item_type_id == itemTypeId && (quantDropped.HasValue
                ? a.quantity_dropped == quantDropped
                : quantDestroy.HasValue && a.quantity_destroyed == quantDestroy));

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                dropped_items item;
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
                    _logger.LogInformation($"Processing Dropped Item for Item ID {item.item_type_id} and Killmail ID {item.killmail_id} and Flag ID {item.flag_id}");

                    AddObjectToDatabase(item);
                }
                catch (DbUpdateException ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Failed inserting Dropped Item for Killmail ID: {KillmailID} and Item {ItemID} - Possible Duplicate Insert", item.killmail_id, item.item_type_id);
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Fatal Exception inserting Dropped Item for Killmail ID: {KillmailID} and Item {ItemID}", item.killmail_id, item.item_type_id);
                }
            }
        }

        private void AddObjectToDatabase(dropped_items obj)
        {
            using var ctx = new KillboardContext(_dbContextOptions);

            if (ctx.dropped_items.Any(a =>
                a.killmail_id == obj.killmail_id && a.item_type_id == obj.item_type_id && (obj.quantity_dropped.HasValue
                    ? a.quantity_dropped == obj.quantity_dropped
                    : obj.quantity_destroyed.HasValue && a.quantity_destroyed == obj.quantity_destroyed))) return;

            ctx.dropped_items.Add(obj);
            ctx.SaveChanges();
        }
    }
}
