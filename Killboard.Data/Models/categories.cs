﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class categories
    {
        public categories()
        {
            groups = new HashSet<groups>();
        }

        public int category_id { get; set; }
        public string name { get; set; }
        public bool? published { get; set; }

        public virtual ICollection<groups> groups { get; set; }
    }
}