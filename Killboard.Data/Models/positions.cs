﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;

namespace Killboard.Data.Models
{
    public partial class positions
    {
        public positions()
        {
            asteroid_belts = new HashSet<asteroid_belts>();
            constellations = new HashSet<constellations>();
            moons = new HashSet<moons>();
            planets = new HashSet<planets>();
            stargates = new HashSet<stargates>();
            systems = new HashSet<systems>();
            victims = new HashSet<victims>();
        }

        public int position_id { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

        public virtual ICollection<asteroid_belts> asteroid_belts { get; set; }
        public virtual ICollection<constellations> constellations { get; set; }
        public virtual ICollection<moons> moons { get; set; }
        public virtual ICollection<planets> planets { get; set; }
        public virtual ICollection<stargates> stargates { get; set; }
        public virtual ICollection<systems> systems { get; set; }
        public virtual ICollection<victims> victims { get; set; }
    }
}