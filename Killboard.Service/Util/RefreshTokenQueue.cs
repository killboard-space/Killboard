using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Killboard.Data.Models;
using Killboard.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Killboard.Service.Util
{
    public class RefreshTokenQueue
    {
        private bool _delegateQueuedOrRunning;

        private readonly ConcurrentQueue<(long charId, string refreshToken)> _objs = new ConcurrentQueue<(long charId, string refreshToken)>();

        private readonly ILogger<RefreshTokenQueue> _logger;
        private readonly IUserService _userService;
        private readonly DbContextOptions<KillboardContext> _dbContextOptions;

        private readonly string _esiClientId;
        private readonly string _esiSecretKey;

        public RefreshTokenQueue(IConfiguration configuration, ILogger<RefreshTokenQueue> logger, IUserService userService)
        {
            _esiClientId = configuration["Killboard:EsiClientId"];
            _esiSecretKey = configuration["Killboard:EsiSecretKey"];

            _logger = logger;
            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>()
                .UseSqlServer(configuration["Killboard:Sql"]).Options;
            _userService = userService;
        }

        public void Enqueue(long charId, string refreshToken)
        {
            lock (_objs)
            {
                _objs.Enqueue((charId, refreshToken));

                if (_delegateQueuedOrRunning) return;

                _delegateQueuedOrRunning = true;
                ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
            }
        }

        public bool IsInQueue(long charId) => _objs.Any(c => c.charId == charId);

        private async void ProcessQueuedItems(object ignored)
        {
            while (true)
            {
                (long charId, string refreshToken) item;
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
                    _logger.LogInformation($"Processing Refresh Token for Character ID {item.charId}");

                    await RefreshToken(item);
                }
                catch (DbUpdateException ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, $"Failed Refreshing Token for Character ID: {item.charId}");
                }
                catch (Exception ex)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(ProcessQueuedItems, null);
                    _logger.LogError(ex, $"Fatal Exception Refreshing Token for Character ID: {item.charId}");
                }
            }
        }

        private async Task RefreshToken((long charId, string refreshToken) obj)
        {
            var callback = await _userService.RefreshToken(_esiClientId, _esiSecretKey, obj.refreshToken);
            if (callback == null) return;

            await using var ctx = new KillboardContext(_dbContextOptions);
            var at = ctx.access_tokens.FirstOrDefault(a => a.refresh_token == obj.refreshToken);

            if (at == null) return;

            at.date_added = DateTime.Now;
            at.expires_on = DateTime.Now.AddSeconds(callback.expires_in);
            at.access_token = callback.access_token;
            at.refresh_token = callback.refresh_token;
            ctx.access_tokens.Update(at);
            await ctx.SaveChangesAsync();
        }
    }
}
