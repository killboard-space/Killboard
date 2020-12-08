using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.Belt
{
    public class GetAsteroidBelt
    {
        public int BeltID { get; set; }
        public string Name{ get; set; }
        public int PlanetID { get; set; }
    }
}
