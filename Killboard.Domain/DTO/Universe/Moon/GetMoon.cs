using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.Moon
{
    public class GetMoon
    {
        public int MoonID { get; set; }
        public string Name { get; set; }
        public int PlanetID { get; set; }
    }
}
