﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class bloodlines
    {
        public bloodlines()
        {
            ancestries = new HashSet<ancestries>();
        }

        public int bloodline_id { get; set; }
        public string name { get; set; }
        public int corporation_id { get; set; }
        public string description { get; set; }
        public int charisma { get; set; }
        public int memory { get; set; }
        public int perception { get; set; }
        public int willpower { get; set; }
        public int ship_type_id { get; set; }
        public int intelligence { get; set; }

        public virtual corporations corporation_ { get; set; }
        public virtual items ship_type_ { get; set; }
        public virtual ICollection<ancestries> ancestries { get; set; }
    }
}