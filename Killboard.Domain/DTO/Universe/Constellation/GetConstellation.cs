using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.Constellation
{
    public class GetConstellation
    {
        public int ConstellationID { get; set; }
        public string Name { get; set; }
        public int RegionID { get; set; }
    }
}
