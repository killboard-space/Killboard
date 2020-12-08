using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Data.Models.StoredProcedures
{
    public class procGetKillmailDetails
    {
        public int killmail_id { get; set; }
        public string hash { get; set; }
        public int victim_character_id { get; set; }
        public string victim_name { get; set; }
        public int corporation_id { get; set; }
        public int? alliance_id { get; set; }
        public string corporation_name { get; set; }
        public string alliance_name { get; set; }
        public int attackers { get; set; }
        public int final_blow_char { get; set; }
        public string final_blow_char_name { get; set; }
        public int ship_type_id { get; set; }
        public string ship_name { get; set; }
        public int system_id { get; set; }
        public string system_name { get; set; }
        public int region_id { get; set; }
        public string region_name { get; set; }
        public DateTime killmail_time { get; set; }
    }
}
