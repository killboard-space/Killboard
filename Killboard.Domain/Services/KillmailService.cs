using Killboard.Data.Models;
using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Enums;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Killboard.Domain.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.EventArgs;

namespace Killboard.Domain.Services
{
    public class KillmailService : IKillmailService
    {
        private readonly KillboardContext _ctx;
        private readonly IHubContext<KillmailHub> _hub;

        private const string TableName = "killmails";

        private static SqlTableDependency<killmails> _tableDependency;

        public KillmailService(KillboardContext ctx, IConfiguration configuration, IHubContext<KillmailHub> hub)
        {
            _ctx = ctx;
            _hub = hub;
            
            _tableDependency = new SqlTableDependency<killmails>(configuration["Killboard:Sql"], TableName);
            _tableDependency.OnChanged += TableDependency_Changed;
            _tableDependency.Start();
        }

        private void TableDependency_Changed(object sender, RecordChangedEventArgs<killmails> e)
        {
            if(e.Entity.finished_processing != null)
                BroadcastKillmail(e.Entity);
        }

        private void BroadcastKillmail(killmails km)
        {
            _hub.Clients.All.SendAsync("NewKillmail", GetAllKillmails(ListTypes.EXACT, km.killmail_id));
        }

        public List<ListDetail> GetAllKillmails(ListTypes type = ListTypes.ALL, int? filter = null)
        {
            return type switch
            {
                ListTypes.ALL => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @killmailId = -1").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.ALLIANCE => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @allianceId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.CHARACTER => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @characterId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.CONSTELLATION => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @constellationId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.CORPORATION => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @corporationId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.GROUP => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @groupId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.REGION => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @regionId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.SHIP => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @shipTypeId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.SYSTEM => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @systemId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                ListTypes.EXACT => _ctx.procGetKillmailDetails.FromSqlInterpolated($"EXEC procGetKillmailDetails @killmailId = {filter}").Select(k => new ListDetail
                {
                    KillmailID = k.killmail_id,
                    KillmailHash = k.hash,
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
                }).ToList(),
                _ => null,
            };
        }

        public void Dispose()
        {
            _tableDependency.Stop();
            _tableDependency.Dispose();
        }
    }
}
