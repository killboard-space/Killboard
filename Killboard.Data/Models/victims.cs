﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>

namespace Killboard.Data.Models
{
    public partial class victims
    {
        public int victim_id { get; set; }
        public int killmail_id { get; set; }
        public int char_id { get; set; }
        public int damage_taken { get; set; }
        public int? faction_id { get; set; }
        public int position_id { get; set; }
        public int ship_type_id { get; set; }

        public virtual characters char_ { get; set; }
        public virtual factions faction_ { get; set; }
        public virtual killmails killmail_ { get; set; }
        public virtual positions position_ { get; set; }
        public virtual items ship_type_ { get; set; }
    }
}