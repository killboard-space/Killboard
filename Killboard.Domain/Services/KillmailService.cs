using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Data.Models;
using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Enums;
using Killboard.Domain.Interfaces;

namespace Killboard.Domain.Services
{
    public class KillmailService : IKillmailService
    {
        private readonly KillboardContext _ctx;

        public KillmailService(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<IEnumerable<ListDetail>> GetAllKillmails(ListTypes type = ListTypes.ALL, int? filter = null)
        {
            return type switch
            {
                ListTypes.ALL => (await _ctx.Procedures.procGetKillmailDetailsAsync(-1, null, null, null, null, null, null, null, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.ALLIANCE => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, filter, null, null, null, null, null, null, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.CHARACTER => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, null, null, filter, null, null, null, null, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.CONSTELLATION => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, null, null, null, null, null, filter, null, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.CORPORATION => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, null, filter, null, null, null, null, null, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.GROUP => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, null, null, null, null, null, null, null, null, filter)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.REGION => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, null, null, null, null, null, null, filter, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.SHIP => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, null, null, null, null, null, null, null, filter, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.SYSTEM => (await _ctx.Procedures.procGetKillmailDetailsAsync(null, null, null, null, null, filter, null, null, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                ListTypes.EXACT => (await _ctx.Procedures.procGetKillmailDetailsAsync(filter, null, null, null, null, null, null, null, null, null)).Select(k => new ListDetail
                {
                    KillmailID = k.KillmailId,
                    KillmailHash = k.KillmailHash,
                    VictimCharacterID = k.VictimCharacterId,
                    VictimName = k.VictimName,
                    VictimAllianceID = k.VictimeAllianceId,
                    VictimCorporationID = k.VictimCorporationId,
                    VictimCorporationName = k.VictimCorporationName,
                    VictimAllianceName = k.VictimAllianceName,
                    FinalBlowID = k.FinalBlowCharId,
                    FinalBlowName = k.FinalBlowCharName,
                    AttackerCount = k.Attackers ?? 0,
                    ShipID = k.ShipTypeId,
                    SystemID = k.SystemId,
                    SystemName = k.SystemName,
                    RegionID = k.RegionId,
                    RegionName = k.RegionName,
                    KillmailTime = k.KillmailTime ?? System.DateTime.Now,
                    ShipName = k.ShipName
                }),
                _ => null,
            };
        }

        public void Dispose()
        {
            _ctx?.Dispose();
        }
    }
}
