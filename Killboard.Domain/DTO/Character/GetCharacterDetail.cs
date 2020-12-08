using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Character
{
    public class GetCharacterDetail : GetCharacter
    {
        public DateTime? Birthday { get; set; }
        public string AllianceName { get; set; }
        public string AllianceTicker { get; set; }
        public string CorporationName { get; set; }
        public string CorporationTicker { get; set; }
        public string Gender { get; set; }
    }
}
