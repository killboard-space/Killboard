﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class ancestries
    {
        public int ancestry_id { get; set; }
        public string name { get; set; }
        public int bloodline_id { get; set; }
        public int? icon_id { get; set; }
        public string short_description { get; set; }
        public string description { get; set; }

        public virtual bloodlines bloodline { get; set; }
    }
}