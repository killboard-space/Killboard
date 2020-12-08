using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.System
{
    public class GetSystem
    {
        public int SystemID { get; set; }
        public string Name{ get; set; }
        public int ConstellationID { get; set; }
        public float Security { get; set; }
    }
}
