using Killboard.Data.Models;
using Killboard.Domain.Models;
using Killboard.Service.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Killboard.Service
{
    /// <summary>
    /// 
    /// </summary>
    public class KillmailWorker : BackgroundService
    {
        private const int MaxRetryCount = 10;
        private const int Delay = 180000;
        private const string EsiUrl = "https://esi.evetech.net/latest/";

        private readonly HttpClient _client = new HttpClient()
        {
            BaseAddress = new Uri(EsiUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };

        private readonly ILogger<KillmailWorker> _logger;
        private readonly IConfiguration _configuration;

        private readonly KillboardQueue _killboardQueue;
        private readonly RefreshTokenQueue _refreshTokenQueue;

        private readonly DbContextOptions<KillboardContext> _dbContextOptions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="userService"></param>
        /// <param name="configuration"></param>
        /// <param name="killboardQueue"></param>
        public KillmailWorker(ILogger<KillmailWorker> logger, IConfiguration configuration,
                      KillboardQueue killboardQueue, RefreshTokenQueue refreshTokenQueue)
        {
            _logger = logger;
            _configuration = configuration;
            _killboardQueue = killboardQueue;
            _refreshTokenQueue = refreshTokenQueue;

            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>().UseSqlServer(_configuration.GetValue<string>("Killboard:Sql")).Options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"[Killboard Service] Service is starting.");

            stoppingToken.Register(() =>
                _logger.LogDebug($"[Killboard Service] Service is being force stopped!"));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"[Killboard Service] Service is running!");

                await DoWork();

                await Task.Delay(Delay, stoppingToken);
            }

            _logger.LogDebug($"[Killboard Service] Service is stopping.");
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task DoWork()
        {
            // Retrieve all tokens that aren't out of date.
            var validTokens = await GetValidTokens();

            // Do nothing if there are no tokens, hopefully not.
            if(validTokens.Count == 0)
            {
                _logger.LogInformation($"[Killboard Service] Failed to find any valid tokens, retrying in {Delay / 1000} seconds.");
            }
            else
            {
                _logger.LogInformation($"[Killboard Service] Found {validTokens.Count} tokens.");

                // Foreach token, concurrently, grouped by the count of tokens divided by 8 (threads)
                // Get new killmails (ids/hashes)

                const int groups = 8;
                var counter = 0;
                Parallel.ForEach(validTokens.GroupBy(t => counter++ % groups), 
                    new ParallelOptions { MaxDegreeOfParallelism = 8 },
                    tp =>
                    {
                        Parallel.ForEach(tp, t =>
                        {
                            try
                            {
                                GetCharacterKillmails(t.Key, t.Value);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "[Killboard Service] Error while getting killmails for Character: {User}", t.Key);
                            }
                        });
                    });
            }
        }

        

        /// <summary>
        /// Attempts to retrieve a list of the most up-to-date killmails for a given character and add relevant ones to the DB.
        /// </summary>
        /// <param name="charId"></param>
        /// <param name="accessToken"></param>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void GetCharacterKillmails(int charId, string accessToken)
        {
            _logger.LogDebug($"[Killboard Service] Getting Killmails for Character: {charId}");

            var success = false;
            var failCount = 0;

            // Set ESI Authentication headers
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Send initial request for killmails
            var response = _client.GetAsync(EsiUrl + "characters/" + charId + "/killmails/recent/").Result;

            while (!success)
            {
                // Success (2xx)
                if (response.IsSuccessStatusCode)
                {
                    // Multiple pages
                    if (response.Headers.TryGetValues("X-Pages", out var vals) && int.TryParse(vals.FirstOrDefault(), out var pages) && pages > 1)
                    {
                        // Existing Killmail IDs for given character.
                        var existingCharKms = GetExistingCharKMIds(charId);

                        // 1 is already done, start at 2 until the end.
                        for (var i = 2; i <= pages; i++)
                        {
                            // Send another paginated request
                            var pageResponse = _client.GetAsync(EsiUrl + "characters/" + charId + $"/killmails/recent/?page={i}").Result;

                            DeserializeAndAddKillmails(pageResponse, existingCharKms);
                        }
                    }
                    else // Single page
                    {
                        DeserializeAndAddKillmails(response);
                    }

                    success = true;
                }
                else // Failed, log & retry?
                {
                    // Probably need to refresh character token
                    if (response.StatusCode == HttpStatusCode.Unauthorized || 
                        (response.StatusCode != HttpStatusCode.NotFound 
                         || response.StatusCode != HttpStatusCode.BadGateway 
                         || response.StatusCode != HttpStatusCode.GatewayTimeout))
                    {

                    }
                    else if (failCount < MaxRetryCount) // Retry
                    {
                        failCount++;
                        var responseMsg = response.Content.ReadAsStringAsync().Result;
                        _logger.LogError($"[Killboard Service] Failed to get killmails for character.\n({failCount}) | {response.StatusCode} | {responseMsg}");
                    }
                    else // Something real is up
                    {
                        _logger.LogError($"[Killboard Service] Failed to get killmails for character after {MaxRetryCount} retries. Will try again on next run.");
                        success = true;
                    }
                }
            }
        }

        /// <summary>
        /// Serializes a raw json string from the ESI Killmail response into DB objects to be enqueued.
        /// </summary>
        /// <param name="response">JSON response returned from Eve Online ESI.</param>
        /// <param name="existingCharKMs">List of existing killmails to not enqueue.</param>
        private void DeserializeAndAddKillmails(HttpResponseMessage response, IEnumerable<int> existingCharKMs = null)
        {
            // Raw JSON string result
            var strResult = response.Content.ReadAsStringAsync().Result;

            // List of Killmail POCOs (unfiltered)
            var jsonResult = JsonConvert.DeserializeObject<List<KillmailModel>>(strResult);

            // Existing Killmail IDs for given character.
            existingCharKMs ??= GetExistingKMIds();

            // Add filtered kms to Queue that adds to DB.
            foreach (var kmToAdd in jsonResult.Where(k => existingCharKMs.All(e => e != k.killmail_id)))
            {
                if (!_killboardQueue.IsInQueue(kmToAdd.killmail_id))
                {
                    _killboardQueue.Enqueue(new killmails 
                    { 
                        hash = kmToAdd.killmail_hash, 
                        killmail_id = kmToAdd.killmail_id,
                        date_added = DateTime.Now
                    });
                }
            }
        }

        /// <summary>
        /// Retrieves a list of the already existing killmail_id's so duplicates aren't added.
        /// </summary>
        /// <param name="charId">The ID for the character who's existing killmails are needed</param>
        /// <remarks>
        /// Primary keys in the DB and/or EF should already enforce this but the use of EF 
        /// and this Framework are not set in stone.
        /// </remarks>
        /// <returns>A list of <see cref="int"/> representing existing.</returns>
        private List<int> GetExistingCharKMIds(int charId)
        {
            using var ctx = new KillboardContext(_dbContextOptions);
            return (from km in ctx.killmails
                    join v in ctx.victims on km.killmail_id equals v.killmail_id
                    where v.char_id == charId
                    select km.killmail_id).ToList();
        }

        private IEnumerable<int> GetExistingKMIds()
        {
            using var ctx = new KillboardContext(_dbContextOptions);
            return (from km in ctx.killmails
                    select km.killmail_id).ToList();
        }

        private void RefreshTokens(IDictionary<int, string> tokens)
        {
            foreach (var (charId, refreshToken) in tokens)
            {
                _refreshTokenQueue.Enqueue(charId, refreshToken);
            }
        }

        /// <summary>
        /// Retrieves all up-to-date access tokens from the database.
        /// </summary>
        /// <returns>
        /// A Dictionary of valid Character Ids & tokens.
        /// </returns>
        private async Task<Dictionary<int, string>> GetValidTokens()
        {
            await using var ctx = new KillboardContext(_dbContextOptions);
            var refreshTokens = ctx.access_tokens
                .Where(a => EF.Functions.DateDiffMinute(DateTime.Now, a.expires_on) + 300 <= 0)
                .ToDictionary(a => a.char_id, a => a.refresh_token);
            
            if(refreshTokens.Count > 0) await Task.Run(() => RefreshTokens(refreshTokens));
            
            return ctx.access_tokens.Where(a => EF.Functions.DateDiffMinute(DateTime.Now, a.expires_on) + 300 > 0)
                .ToDictionary(a => a.char_id, a => a.access_token);
        }
    }
}
