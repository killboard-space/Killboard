using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Killboard.Data.Models;
using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Models;
using Killboard.Service.Interfaces;
using Killboard.Service.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace Killboard.Service.Services
{
    public class KillmailTableSubscription : IDatabaseSubscription
    {
        private bool _disposedValue;
        private const string EsiUrl = "https://esi.evetech.net/latest/";

        private readonly HttpClient _client = new HttpClient()
        {
            BaseAddress = new Uri(EsiUrl),
            Timeout = TimeSpan.FromSeconds(15)
        };

        private SqlTableDependency<killmails> _tableDependency;

        private readonly ILogger<KillmailTableSubscription> _logger;
        private readonly VictimQueue _victimQueue;
        private readonly AttackerQueue _attackerQueue;
        private readonly DroppedItemQueue _itemQueue;
        private readonly CharacterQueue _characterQueue;
        private readonly CorporationQueue _corpQueue;
        private readonly AllianceQueue _allianceQueue;

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
        }

        #region SqlTableDependency

        public void Configure(string connectionString)
        {
            _tableDependency = new SqlTableDependency<killmails>(connectionString);
            _tableDependency.OnChanged += Changed;
            _tableDependency.OnError += TableDependency_OnError;
            _tableDependency.Start();

            Console.WriteLine("[SQLTableDependency] Waiting for receiving notifications...");
        }

        private void TableDependency_OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogError($"[SQLTableDependency] Error: {e.Error.Message}");
        }

        private async void Changed(object sender, RecordChangedEventArgs<killmails> e)
        {
            if (e.ChangeType == ChangeType.Insert) return;
            var newEntity = e.Entity;
            _logger.LogInformation($"[SQLTableDependency] Found new Killmail: {newEntity.killmail_id} | {newEntity.hash}");
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

                        await AddVicitim(killmailDetail.Victim, killmailId);

                        await AddAttackers(killmailDetail.Attackers, killmailId);

                        if (killmailDetail.Victim.Items != null && killmailDetail.Victim.Items.Length > 0)
                            AddDroppedItems(killmailDetail.Victim.Items, killmailId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);

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

        private async Task GetPublicData(int characterId, bool isCreatorOrCEO = false)
        {
            var response = await _client.GetAsync($"{EsiUrl}characters/{characterId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<PublicDataModel>(jsonStr);

                    if (data != null) await AddPublicData(data, characterId, isCreatorOrCEO);
                }
            }
            else
            {
                _logger.LogError($"[KillmailDetail Service] Error while getting public data CharacterID: {characterId} | {response.ReasonPhrase}");
            }
        }

        private async Task GetAlliancePublicData(int allianceId)
        {
            var response = await _client.GetAsync($"{EsiUrl}alliances/{allianceId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<AlliancePublicData>(jsonStr);

                    if (data != null) await AddAllianceData(data, allianceId);
                }
            }
            else
            {
                _logger.LogError($"[KillmailDetail Service] Error while getting public data AllianceID: {allianceId} | {response.ReasonPhrase}");
            }
        }

        private async Task GetCorporationPublicData(int corporationId)
        {
            var response = await _client.GetAsync($"{EsiUrl}corporations/{corporationId}");

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();

                if (jsonStr.Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<CorporationPublicData>(jsonStr);

                    if (data != null) await AddCorporationPublicData(data, corporationId);
                }
            }
            else
            {
                _logger.LogError($"[KillmailDetail Service] Error while getting public data CorporationID: {corporationId} | {response.ReasonPhrase}");
            }
        }

        #endregion

        #region DB Queries

        private async Task AddPublicData(PublicDataModel data, int characterId, bool isCreatorOrCeo = false)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (data.alliance_id.HasValue && !ctx.alliances.Any(a => a.alliance_id == data.alliance_id) && !_allianceQueue.IsInQueue(data.alliance_id.Value))
                await GetAlliancePublicData((int)data.alliance_id);

            if (!isCreatorOrCeo && !ctx.corporations.Any(c => c.corporation_id == data.corporation_id) && !_corpQueue.IsInQueue(data.corporation_id))
                await GetCorporationPublicData(data.corporation_id);

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

        private async Task AddCorporationPublicData(CorporationPublicData data, int corporationId)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (data.AllianceID.HasValue && !ctx.alliances.Any(a => a.alliance_id == data.AllianceID) && !_allianceQueue.IsInQueue(data.AllianceID.Value))
                await GetAlliancePublicData(data.AllianceID.Value);

            if (!ctx.characters.Any(c => c.character_id == data.CeoID) && !_characterQueue.IsInQueue(data.CeoID))
                await GetPublicData(data.CeoID, true);

            if (ctx.characters.Any(c => c.character_id == data.CeoID && c.corporation_id != corporationId))
                await UpdateCharacterLoyalty(data.CeoID, corporationId, data.AllianceID.GetValueOrDefault());

            if (!ctx.characters.Any(c => c.character_id == data.CreatorID) && !_characterQueue.IsInQueue(data.CreatorID))
                await GetPublicData(data.CreatorID, true);

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

        private async Task AddAllianceData(AlliancePublicData data, int allianceId)
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
                    await GetCorporationPublicData(data.CreatorCorporationID);

                // Add Founder
                if (!ctx.characters.Any(c => c.character_id == data.CreatorID) && !_characterQueue.IsInQueue(data.CreatorID))
                    await GetPublicData(data.CreatorID, true);

                // Add Executor Corp
                if (data.ExecutorCorporationID.HasValue && !ctx.corporations.Any(c => c.corporation_id == data.ExecutorCorporationID) && !_corpQueue.IsInQueue(data.ExecutorCorporationID.Value))
                    await GetCorporationPublicData(data.ExecutorCorporationID.Value);
            }
        }

        private async Task AddVicitim(Victim victim, int killmailId)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (!ctx.characters.Any(c => c.character_id == victim.CharacterId) && !_characterQueue.IsInQueue((int)victim.CharacterId))
                await GetPublicData((int)victim.CharacterId);

            if (ctx.characters.Any(c => c.character_id == victim.CharacterId && c.corporation_id != victim.CorporationId))
                await UpdateCharacterLoyalty((int)victim.CharacterId, (int)victim.CorporationId, (int)victim.AllianceId.GetValueOrDefault());

            if (!ctx.corporations.Any(c => c.corporation_id == victim.CorporationId) && !_corpQueue.IsInQueue((int)victim.CorporationId))
                await GetCorporationPublicData((int)victim.CorporationId);

            if (victim.AllianceId.HasValue && !ctx.alliances.Any(a => a.alliance_id == victim.AllianceId) && !_allianceQueue.IsInQueue((int)victim.AllianceId.Value))
                await GetAlliancePublicData((int)victim.AllianceId.Value);

            if (!_victimQueue.IsInQueue(killmailId, (int)victim.CharacterId))
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
                            char_id = (int)victim.CharacterId,
                            damage_taken = (int)victim.DamageTaken,
                            killmail_id = killmailId,
                            ship_type_id = (int)victim.ShipTypeId,
                            position_id = positionEntry.Entity.position_id
                        });
                    }
                }
            }
        }

        private async Task AddAttackers(IEnumerable<Attacker> attackers, int killmailId)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            foreach (var attacker in attackers)
            {
                if (!ctx.characters.Any(c => c.character_id == attacker.CharacterId) && !_characterQueue.IsInQueue((int)attacker.CharacterId))
                    await GetPublicData((int)attacker.CharacterId);

                if (ctx.characters.Any(c => c.character_id == attacker.CharacterId && c.corporation_id != attacker.CorporationId))
                    await UpdateCharacterLoyalty((int)attacker.CharacterId, (int)attacker.CorporationId, (int)attacker.AllianceId);

                if (!ctx.corporations.Any(c => c.corporation_id == attacker.CorporationId) && !_corpQueue.IsInQueue((int)attacker.CorporationId))
                    await GetCorporationPublicData((int)attacker.CorporationId);

                if (attacker.AllianceId != default && !ctx.alliances.Any(a => a.alliance_id == attacker.AllianceId) && !_allianceQueue.IsInQueue((int)attacker.AllianceId))
                    await GetAlliancePublicData((int)attacker.AllianceId);

                if (!_attackerQueue.IsInQueue((int)attacker.CharacterId, killmailId))
                {
                    _attackerQueue.Enqueue(new attackers
                    {
                        killmail_id = killmailId,
                        char_id = (int)attacker.CharacterId,
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

        private async Task UpdateCharacterLoyalty(int characterId, int corporationId, int allianceId)
        {
            await using var ctx = new KillboardContext(_dbContextOptions);

            if (!ctx.corporations.Any(c => c.corporation_id == corporationId) && !_corpQueue.IsInQueue(corporationId))
                await GetCorporationPublicData(corporationId);

            if (allianceId != default && !ctx.alliances.Any(a => a.alliance_id == allianceId) && !_allianceQueue.IsInQueue(allianceId))
                await GetAlliancePublicData(allianceId);

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
