﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>

namespace Killboard.Data.Models
{
    public partial class attackers
    {
        public int attacker_id { get; set; }
        public long? char_id { get; set; }
        public int? corporation_id { get; set; }
        public int? alliance_id { get; set; }
        public int damage_done { get; set; }
        public bool final_blow { get; set; }
        public int ship_type_id { get; set; }
        public int weapon_type_id { get; set; }
        public int killmail_id { get; set; }

        public virtual killmails killmail_ { get; set; }
        public virtual items ship_type_ { get; set; }
        public virtual items weapon_type_ { get; set; }
    }
}