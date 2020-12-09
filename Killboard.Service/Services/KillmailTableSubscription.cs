using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Killboard.Data.Models;
using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Models;
using Killboard.Service.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace Killboard.Service.Services
{
    public class KillmailTableSubscription : IHostedService
    {
        private bool _disposedValue;
        private const string EsiUrl = "https://esi.evetech.net/latest/";

        private readonly HttpClient _client = new HttpClient()
        {
            BaseAddress = new Uri(EsiUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };

        private readonly ILogger<KillmailTableSubscription> _logger;

        private readonly VictimQueue _victimQueue;
        private readonly AttackerQueue _attackerQueue;
        private readonly DroppedItemQueue _itemQueue;
        private readonly CharacterQueue _characterQueue;
        private readonly CorporationQueue _corpQueue;
        private readonly AllianceQueue _allianceQueue;

        private readonly SqlTableDependency<killmails> _tableDependency;
        private readonly DbContextOptions<KillboardContext> _dbContextOptions;

        public KillmailTableSubscription(ILogger<KillmailTableSubscription> logger, IConfiguration configuration, 
            VictimQueue victimQueue, AttackerQueue attackerQueue,
            DroppedItemQueue itemQueue, CharacterQueue characterQueue, 
            CorporationQueue corpQueue, AllianceQueue allianceQueue)
        {
            _logger = logger;
            _victimQueue = victimQueue;
            _attackerQueue = attackerQueue;
            _itemQueue = itemQueue;
            _characterQueue = characterQueue;
            _corpQueue = corpQueue;
            _allianceQueue = allianceQueue;

            _dbContextOptions = new DbContextOptionsBuilder<KillboardContext>().UseSqlServer(configuration["Killboard:Sql"]).Options;

            _tableDependency = new SqlTableDependency<killmails>(configuration["Killboard:Sql"], "killmails");
            _tableDependency.OnChanged += Changed;
            _tableDependency.OnError += TableDependency_OnError;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _tableDependency.Start();
            _logger.LogInformation("[SQLTableDependency] Waiting to receive notifications...");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _tableDependency.Stop();
            _logger.LogInformation("[SQLTableDependency] Shutting down...");

            return Task.CompletedTask;
        }

        #region SqlTableDependency

        private void TableDependency_OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogError($"[SQLTableDependency] Error: {e.Error.Message}");
        }

        private async void Changed(object sender, RecordChangedEventArgs<killmails> e)
        {
            if (e.ChangeType != ChangeType.Insert) return;

            var newEntity = e.Entity;
            _logger.LogInformation($"[SQLTableDependency] New Killmail: {newEntity.killmail_id} | {newEntity.hash}");
            await GetKMDetail(newEntity.killmail_id, newEntity.hash);
        }

        #endregion

        #region ESI Requests
        // TODO: Handle Public Data for characters and how/if it should persist
        // or stay update to date with current character info.
        private async Task GetKMDetail(int killmailId, string hash)
        {
            var response = await _client.GetAsync($"{EsiUrl}killmails/{killmailId}/{hash}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Length > 0)
                {
                    var killmailDetail = JsonConvert.DeserializeObject<KillmailDetail>(jsonStr);

                    try
                    {
                        StartProcessingKillmail(killmailId, killmailDetail.KillmailTime.DateTime,
                            (int) killmailDetail.SolarSystemId);

                        await AddVicitim(killmailDetail.Victim, killmailId, hash);

                        await AddAttackers(killmailDetail.Attackers, killmailId, hash);

                        if (killmailDetail.Victim.Items != null && killmailDetail.Victim.Items.Length > 0)
                            AddDroppedItems(killmailDetail.Victim.Items, killmailId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"[KillmailDetail Service] Failed Processing Killmail: {killmailId} | {hash}\n\tMessage:{ex.Message}\nTrace:{ex.StackTrace}");

                        ResetKillmail(killmailId);
                    }
                    finally
                    {
                        await MarkKillmailAsComplete(killmailId, hash);
                    }
                }
            }
            else
            {
                _logger.LogError($"[KillmailDetail Service] Error while getting killmail detail for Reason: {response.ReasonPhrase}");
            }
        }

        private async Task GetPublicData(long characterId, int killmailId, string hash, bool isCreatorOrCEO = false)
        {
            var response = await _client.GetAsync($"{EsiUrl}characters/{characterId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<PublicDataModel>(jsonStr);

                    if (data != null) await AddPublicData(data, characterId, killmailId, hash, isCreatorOrCEO);
                }
            }
            else
            {
                _logger.LogError($"[KillmailDetail Service] Error while getting public data CharacterID: {characterId} | Id: {killmailId} | Hash: {hash} | {response.ReasonPhrase}");
            }
        }

        private async Task GetAlliancePublicData(int allianceId, int killmailId, string hash)
        {
            var response = await _client.GetAsync($"{EsiUrl}alliances/{allianceId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<AlliancePublicData>(jsonStr);

                    if (data != null) await AddAllianceData(data, allianceId, killmailId, hash);
                }
            }
            else
            {
                _logger.LogError($"[KillmailDetail Service] Error while getting public data AllianceID: {allianceId} | {response.ReasonPhrase}");
            }
        }

        private async Task GetCorporationPublicData(int corporationId, int killmailId, string hash)
        {
            var response = await _client.GetAsync($"{EsiUrl}corporations/{corporationId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<CorporationPublicData>(jsonStr);

                    if (data != null) await AddCorporationPublicData(data, corporationId, killmailId, hash);
                }
            }
            else
            {
                _logger.LogError($"[KillmailDetail Service] Error while getting public data CorporationID: {corporationId} | {response.ReasonPhrase}");
            }
        }

        #endregion

        #region DB Queries

        private async Task AddPublicData(PublicDataModel data, long characterId, int killmailId, string hash, bool isCreatorOrCeo = false)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (data.alliance_id.HasValue && !ctx.alliances.Any(a => a.alliance_id == data.alliance_id) && !_allianceQueue.IsInQueue(data.alliance_id.Value))
                await GetAlliancePublicData((int)data.alliance_id, killmailId, hash);

            if (!isCreatorOrCeo && !ctx.corporations.Any(c => c.corporation_id == data.corporation_id) && !_corpQueue.IsInQueue(data.corporation_id))
                await GetCorporationPublicData(data.corporation_id, killmailId, hash);

            if (!_characterQueue.IsInQueue(characterId))
            {
                _characterQueue.Enqueue(new characters
                {
                    alliance_id = data.alliance_id,
                    description = data.description,
                    ancestry_id = data.acestry_id,
                    bloodline_id = data.bloodline_id,
                    character_id = characterId,
                    corporation_id = data.corporation_id,
                    gender = data.gender,
                    name = data.name,
                    race_id = data.race_id,
                    security_status = data.security_status,
                    birthday = !string.IsNullOrEmpty(data.birthday) && DateTime.TryParse(data.birthday, out var birthday) ? birthday : SqlDateTime.MinValue.Value
                });
            }
        }

        private async Task AddCorporationPublicData(CorporationPublicData data, int corporationId, int killmailId, string hash)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (data.AllianceID.HasValue && !ctx.alliances.Any(a => a.alliance_id == data.AllianceID) && !_allianceQueue.IsInQueue(data.AllianceID.Value))
                await GetAlliancePublicData(data.AllianceID.Value, killmailId, hash);

            if (data.CeoID.HasValue && !ctx.characters.Any(c => c.character_id == data.CeoID) && !_characterQueue.IsInQueue(data.CeoID.Value))
                await GetPublicData(data.CeoID.Value, killmailId, hash, true);

            if (data.CeoID.HasValue && ctx.characters.Any(c => c.character_id == data.CeoID && c.corporation_id != corporationId))
                await UpdateCharacterLoyalty(data.CeoID.Value, corporationId, data.AllianceID.GetValueOrDefault(), killmailId, hash);

            if (data.CreatorID.HasValue && !ctx.characters.Any(c => c.character_id == data.CreatorID) && !_characterQueue.IsInQueue(data.CreatorID.Value))
                await GetPublicData(data.CreatorID.Value, killmailId, hash, true);

            if (!_corpQueue.IsInQueue(corporationId))
            {
                _corpQueue.Enqueue(new corporations
                {
                    alliance_id = data.AllianceID,
                    ceo_char_id = data.CeoID,
                    description = data.Description,
                    corporation_id = corporationId,
                    create_char = data.CreatorID,
                    create_time = data.DateFounded ?? SqlDateTime.MinValue.Value,
                    member_count = data.MemberCount,
                    name = data.Name,
                    tax_rate = (decimal?)data.TaxRate,
                    ticker = data.Ticker,
                    url = data.Url
                });
            }
        }

        private async Task AddAllianceData(AlliancePublicData data, int allianceId, int killmailId, string hash)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (!_allianceQueue.IsInQueue(allianceId))
            {
                _allianceQueue.Enqueue(new alliances
                {
                    alliance_id = allianceId,
                    executor_corp_id = data.ExecutorCorporationID,
                    name = data.Name,
                    ticker = data.Ticker
                });

                // Add Founding Corp
                if (!ctx.corporations.Any(c => c.corporation_id == data.CreatorCorporationID) && !_corpQueue.IsInQueue(data.CreatorCorporationID))
                    await GetCorporationPublicData(data.CreatorCorporationID, killmailId, hash);

                // Add Founder
                if (data.CreatorID.HasValue && !ctx.characters.Any(c => c.character_id == data.CreatorID) && !_characterQueue.IsInQueue(data.CreatorID.Value))
                    await GetPublicData(data.CreatorID.Value, killmailId, hash, true);

                // Add Executor Corp
                if (data.ExecutorCorporationID.HasValue && !ctx.corporations.Any(c => c.corporation_id == data.ExecutorCorporationID) && !_corpQueue.IsInQueue(data.ExecutorCorporationID.Value))
                    await GetCorporationPublicData(data.ExecutorCorporationID.Value, killmailId, hash);
            }
        }

        private async Task AddVicitim(Victim victim, int killmailId, string hash)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (victim.CharacterId.HasValue && !ctx.characters.Any(c => c.character_id == victim.CharacterId) && !_characterQueue.IsInQueue(victim.CharacterId.Value))
                await GetPublicData(victim.CharacterId.Value, killmailId, hash);

            if (victim.CharacterId.HasValue && victim.CorporationId.HasValue && victim.AllianceId.HasValue && !ctx.characters.Any(c => c.character_id == victim.CharacterId && c.corporation_id != victim.CorporationId))
                await UpdateCharacterLoyalty(victim.CharacterId.Value, victim.CorporationId.Value, victim.AllianceId.Value, killmailId, hash);

            if (victim.CorporationId.HasValue && !ctx.corporations.Any(c => c.corporation_id == victim.CorporationId) && !_corpQueue.IsInQueue((int)victim.CorporationId))
                await GetCorporationPublicData(victim.CorporationId.Value, killmailId, hash);

            if (victim.AllianceId.HasValue && !ctx.alliances.Any(a => a.alliance_id == victim.AllianceId) && !_allianceQueue.IsInQueue((int)victim.AllianceId.Value))
                await GetAlliancePublicData(victim.AllianceId.Value, killmailId, hash);

            if (victim.CharacterId.HasValue && !_victimQueue.IsInQueue(killmailId, victim.CharacterId.Value))
            {
                // The time in between checking the Queue, Querying the DB and then continuing is worrying.
                var km = ctx.killmails.FirstOrDefault(k => k.killmail_id == killmailId);

                if (km != null)
                {
                    var positionEntry = await ctx.positions.AddAsync(new positions
                    {
                        x = victim.Position.X,
                        y = victim.Position.Y,
                        z = victim.Position.Z
                    });
                    await ctx.SaveChangesAsync();

                    if (positionEntry.IsKeySet)
                    {
                        _victimQueue.Enqueue(new victims
                        {
                            char_id = victim.CharacterId,
                            corporation_id = victim.CorporationId,
                            alliance_id = victim.AllianceId,
                            damage_taken = (int)victim.DamageTaken,
                            killmail_id = killmailId,
                            ship_type_id = (int)victim.ShipTypeId,
                            position_id = positionEntry.Entity.position_id
                        });
                    }
                }
            }
        }

        private async Task AddAttackers(IEnumerable<Attacker> attackers, int killmailId, string hash)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            foreach (var attacker in attackers)
            {
                if (attacker.CharacterId.HasValue && !ctx.characters.Any(c => c.character_id == attacker.CharacterId) && !_characterQueue.IsInQueue(attacker.CharacterId.Value))
                    await GetPublicData(attacker.CharacterId.Value, killmailId, hash);

                if (attacker.CharacterId.HasValue && attacker.CorporationId.HasValue && attacker.AllianceId.HasValue && ctx.characters.Any(c => c.character_id == attacker.CharacterId && c.corporation_id != attacker.CorporationId))
                    await UpdateCharacterLoyalty(attacker.CharacterId.Value, attacker.CorporationId.Value, attacker.AllianceId.Value, killmailId, hash);

                if (attacker.CorporationId.HasValue && !ctx.corporations.Any(c => c.corporation_id == attacker.CorporationId) && !_corpQueue.IsInQueue(attacker.CorporationId.Value))
                    await GetCorporationPublicData(attacker.CorporationId.Value, killmailId, hash);

                if (attacker.AllianceId.HasValue && !ctx.alliances.Any(a => a.alliance_id == attacker.AllianceId) && !_allianceQueue.IsInQueue(attacker.AllianceId.Value))
                    await GetAlliancePublicData(attacker.AllianceId.Value, killmailId, hash);

                if (!_attackerQueue.IsInQueue(killmailId, attacker.CharacterId, attacker.CorporationId, attacker.AllianceId))
                {
                    _attackerQueue.Enqueue(new attackers
                    {
                        killmail_id = killmailId,
                        char_id = attacker.CharacterId,
                        corporation_id = attacker.CorporationId,
                        alliance_id = attacker.AllianceId,
                        damage_done = (int)attacker.DamageDone,
                        final_blow = attacker.FinalBlow,
                        ship_type_id = (int)attacker.ShipTypeId,
                        weapon_type_id = (int)attacker.WeaponTypeId
                    });
                }
            }
        }

        private void AddDroppedItems(IEnumerable<Item> items, int killmailId)
        {
            var itemsToAdd = items.Where(a => !_itemQueue.IsInQueue(killmailId, a.ItemTypeId, a.QuantityDropped, a.QuantityDestroyed))
                .Select(i => new dropped_items
                {
                    quantity_destroyed = i.QuantityDestroyed,
                    quantity_dropped = i.QuantityDropped,
                    flag_id = i.Flag,
                    item_type_id = i.ItemTypeId,
                    killmail_id = killmailId
                }).ToList();

            itemsToAdd.ForEach(a => _itemQueue.Enqueue(a));
        }

        private async Task UpdateCharacterLoyalty(long characterId, int corporationId, int allianceId, int killmailId, string hash)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (!ctx.corporations.Any(c => c.corporation_id == corporationId) && !_corpQueue.IsInQueue(corporationId))
                await GetCorporationPublicData(corporationId, killmailId, hash);

            if (allianceId != default && !ctx.alliances.Any(a => a.alliance_id == allianceId) && !_allianceQueue.IsInQueue(allianceId))
                await GetAlliancePublicData(allianceId, killmailId, hash);

            var character = ctx.characters.FirstOrDefault(c => c.character_id == characterId);

            if (character != null)
            {
                character.corporation_id = corporationId;
                character.alliance_id = allianceId;
                ctx.characters.Update(character);
                await ctx.SaveChangesAsync();
            }
        }

        private void StartProcessingKillmail(int killmailId, DateTime killmailTime, int systemId)
        {
            using var ctx = new KillboardContext(_dbContextOptions);

            var km = ctx.killmails.FirstOrDefault(k => k.killmail_id == killmailId);
            if (km == null) return;

            km.started_processing = DateTime.Now;
            km.system_id = systemId;
            km.killmail_time = killmailTime;
            ctx.killmails.Update(km);
            ctx.SaveChanges();
        }

        private async Task MarkKillmailAsComplete(int killmailId, string hash)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);
            var km = ctx.killmails.FirstOrDefault(k => k.killmail_id == killmailId);

            if (km != null)
            {
                var listDetail = ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails {killmailId}").ToList().Select(k => new ListDetail
                {
                    KillmailID = killmailId,
                    KillmailHash = hash,
                    VictimCharacterID = k.victim_character_id,
                    VictimName = k.victim_name,
                    VictimAllianceID = k.alliance_id,
                    VictimCorporationID = k.corporation_id,
                    VictimCorporationName = k.corporation_name,
                    VictimAllianceName = k.alliance_name,
                    FinalBlowID = k.final_blow_char,
                    FinalBlowName = k.final_blow_char_name,
                    AttackerCount = k.attackers,
                    ShipID = k.ship_type_id,
                    SystemID = k.system_id,
                    SystemName = k.system_name,
                    RegionID = k.region_id,
                    RegionName = k.region_name,
                    KillmailTime = k.killmail_time,
                    ShipName = k.ship_name
                }).FirstOrDefault();

                if (listDetail != default)
                {
                    km.finished_processing = DateTime.Now;
                    ctx.killmails.Update(km);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        private void ResetKillmail(int killmailId)
        {
            using var ctx = new KillboardContext(_dbContextOptions);
            ctx.Database.ExecuteSqlInterpolated($"UPDATE killmails set started_processing = null, finished_processing = null where killmail_id = {killmailId}");
        }

        #endregion

        #region IDisposable

        ~KillmailTableSubscription()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _tableDependency.Stop();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
