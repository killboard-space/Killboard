using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Alliance
{
    public class GetAlliance
    {
        public int AllianceID { get; set; }
        public string Name { get; set; }
        public string Ticker { get; set; }
    }
}
