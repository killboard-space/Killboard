﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class corporations
    {
        public corporations()
        {
            bloodlines = new HashSet<bloodlines>();
            characters = new HashSet<characters>();
            factions = new HashSet<factions>();
        }

        public string name { get; set; }
        public int? alliance_id { get; set; }
        public int? ceo_char_id { get; set; }
        public string description { get; set; }
        public string ticker { get; set; }
        public DateTime create_time { get; set; }
        public int? create_char { get; set; }
        public int member_count { get; set; }
        public decimal? tax_rate { get; set; }
        public string url { get; set; }
        public int corporation_id { get; set; }

        public virtual alliances alliance_ { get; set; }
        public virtual ICollection<bloodlines> bloodlines { get; set; }
        public virtual ICollection<characters> characters { get; set; }
        public virtual ICollection<factions> factions { get; set; }
    }
}