﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class victims
    {
        public int victim_id { get; set; }
        public int killmail_id { get; set; }
        public long? char_id { get; set; }
        public int damage_taken { get; set; }
        public int? faction_id { get; set; }
        public int position_id { get; set; }
        public int ship_type_id { get; set; }
        public int? alliance_id { get; set; }
        public int? corporation_id { get; set; }

        public virtual characters _char { get; set; }
        public virtual alliances alliance { get; set; }
        public virtual corporations corporation { get; set; }
        public virtual factions faction { get; set; }
        public virtual killmails killmail { get; set; }
        public virtual positions position { get; set; }
        public virtual items ship_type { get; set; }
    }
}