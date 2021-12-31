﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class dogma_effects
    {
        public dogma_effects()
        {
            dogma_modifiers = new HashSet<dogma_modifiers>();
            item_effects = new HashSet<item_effects>();
        }

        public int effect_id { get; set; }
        public string description { get; set; }
        public bool? disallow_auto_repeat { get; set; }
        public int? dischard_attribute_id { get; set; }
        public string display_name { get; set; }
        public int? duration_attribute_id { get; set; }
        public int? effect_gategory { get; set; }
        public bool? electronic_chance { get; set; }
        public int? falloff_attribute_id { get; set; }
        public int? icon_id { get; set; }
        public bool? is_assistance { get; set; }
        public bool? is_offensive { get; set; }
        public bool? is_warp_safe { get; set; }
        public string name { get; set; }
        public int? post_expression { get; set; }
        public int? pre_expression { get; set; }
        public bool? published { get; set; }
        public int? range_attribute_id { get; set; }
        public bool? range_chance { get; set; }
        public int? tracking_speed_attribute_id { get; set; }

        public virtual ICollection<dogma_modifiers> dogma_modifiers { get; set; }
        public virtual ICollection<item_effects> item_effects { get; set; }
    }
}