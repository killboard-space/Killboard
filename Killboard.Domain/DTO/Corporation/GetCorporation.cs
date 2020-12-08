using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Corporation
{
    public class GetCorporation
    {
        public int CorporationID { get; set; }
        public string Name { get; set; }
        public int? AllianceID { get; set; }
        public string Description { get; set; }
        public string Ticker { get; set; }
    }
}
