﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class factions
    {
        public factions()
        {
            victims = new HashSet<victims>();
        }

        public int faction_id { get; set; }
        public string name { get; set; }
        public int? corporation_id { get; set; }
        public string description { get; set; }
        public bool is_unique { get; set; }
        public double size_factor { get; set; }
        public int system_id { get; set; }
        public int station_count { get; set; }
        public int station_system_count { get; set; }
        public int? militia_corporation_id { get; set; }

        public virtual corporations corporation_ { get; set; }
        public virtual systems system_ { get; set; }
        public virtual ICollection<victims> victims { get; set; }
    }
}