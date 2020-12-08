using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Killmail
{
    public class ListDetail
    {
        public int KillmailID { get; set; }
        public string KillmailHash { get; set; }
        public DateTime KillmailTime { get; set; }
        public string SystemName { get; set; }
        public int SystemID { get; set; }
        public string RegionName { get; set; }
        public int RegionID { get; set; }
        public string ShipName { get; set; }
        public int ShipID { get; set; }
        public string VictimName { get; set; }
        public int VictimCharacterID { get; set; }
        public string VictimCorporationName { get; set; }
        public int VictimCorporationID { get; set; }
        public string VictimAllianceName { get; set; }
        public int? VictimAllianceID { get; set; }
        public string FinalBlowName { get; set; }
        public int FinalBlowID { get; set; }
        public int AttackerCount { get; set; }
    }
}
