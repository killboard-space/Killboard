using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Alliance
{
    public class GetAllianceDetail : GetAlliance
    {
        public int? ExecutorCorpID { get; set; }
        public string ExecutorCorpName { get; set; }
    }
}
