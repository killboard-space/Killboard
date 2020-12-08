using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.Planet
{
    public class GetPlanet
    {
        public int PlanetID { get; set; }
        public string Name { get; set; }
        public int SystemID { get; set; }
    }
}
