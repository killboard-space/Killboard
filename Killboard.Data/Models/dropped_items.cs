﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>

namespace Killboard.Data.Models
{
    public partial class dropped_items
    {
        public int flag_id { get; set; }
        public int item_type_id { get; set; }
        public int? quantity_dropped { get; set; }
        public int? quantity_destroyed { get; set; }
        public int killmail_id { get; set; }

        public virtual items item_type_ { get; set; }
        public virtual killmails killmail_ { get; set; }
    }
}