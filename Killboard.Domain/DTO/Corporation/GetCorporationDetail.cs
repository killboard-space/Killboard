using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Corporation
{
    public class GetCorporationDetail : GetCorporation
    {
        public int? ExecutorCorpID { get; set; }
        public string ExecutorCorpName { get; set; }
        public string AllianceName { get; set; }
        public string AllianceTicker { get; set; }
    }
}
