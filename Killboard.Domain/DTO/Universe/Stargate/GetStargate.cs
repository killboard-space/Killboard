using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.Stargate
{
    public class GetStargate
    {
        public int StargateID { get; set; }
        public string Name { get; set; }
        public int SystemID { get; set; }
        public int DestinationStargateID { get; set; }
    }
}
