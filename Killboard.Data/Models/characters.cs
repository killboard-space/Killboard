﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class characters
    {
        public characters()
        {
            access_tokens = new HashSet<access_tokens>();
            character_scopes = new HashSet<character_scopes>();
            victims = new HashSet<victims>();
        }

        public int? user_id { get; set; }
        public string name { get; set; }
        public int corporation_id { get; set; }
        public float security_status { get; set; }
        public string description { get; set; }
        public int bloodline_id { get; set; }
        public string gender { get; set; }
        public int race_id { get; set; }
        public DateTime birthday { get; set; }
        public int ancestry_id { get; set; }
        public long character_id { get; set; }
        public int? alliance_id { get; set; }

        public virtual alliances alliance_ { get; set; }
        public virtual corporations corporation_ { get; set; }
        public virtual users user_ { get; set; }
        public virtual ICollection<access_tokens> access_tokens { get; set; }
        public virtual ICollection<character_scopes> character_scopes { get; set; }
        public virtual ICollection<victims> victims { get; set; }
    }
}