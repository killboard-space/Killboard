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
    public class CorporationQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly ConcurrentQueue<corporations> _objs = new ConcurrentQueue<corporations>();

        private readonly ILogger<CorporationQueue> _logger;
        private readonly DbContextOptions<KillboardContext> _dbContextOptions;

        public CorporationQueue(ILogger<CorporationQueue> logger, IConfiguration configuration)
        {
            _logger = logger;
            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>()
                .UseSqlServer(configuration["Killboard:Sql"]).Options;
        }

        public void Enqueue(corporations obj)
        {
            lock (_objs)
            {
                _objs.Enqueue(obj);
                
                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(int corpId) => _objs.Any(a => a.corporation_id == corpId);

        private void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                corporations item;
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
                    _logger.LogInformation($"Processing Corporation for Corporation ID {item.corporation_id}");

                    AddObjectToDatabase(item);
                }
                catch (DbUpdateException ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Failed inserting Corporation for Corporation ID: {CorporationID} - Possible Duplicate Insert", item.corporation_id);
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, "Fatal Exception inserting Corporation for Corporation ID: {CorporationID}", item.corporation_id);
                }
            }
        }

        private void AddObjectToDatabase(corporations obj)
        {
            using var ctx = new KillboardContext(_dbContextOptions);
            
            if (ctx.corporations.Any(k => k.corporation_id == obj.corporation_id)) return;

            ctx.corporations.Add(obj);
            ctx.SaveChanges();
        }
    }
}
