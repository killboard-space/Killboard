﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class regions
    {
        public regions()
        {
            constellations = new HashSet<constellations>();
        }

        public int region_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public virtual ICollection<constellations> constellations { get; set; }
    }
}