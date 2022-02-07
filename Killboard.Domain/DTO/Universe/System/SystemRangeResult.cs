using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.System
{
    public class SystemRangeResult
    {
        public int SystemId { get; set; }
        public string Name { get; set; }
        public float SecurityStatus { get; set; }
        public double? Distance { get; set; }
    }
}
