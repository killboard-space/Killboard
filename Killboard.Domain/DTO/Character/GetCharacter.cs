using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Character
{
    public class GetCharacter
    {
        public int CharacterID{ get; set; }
        public string Username { get; set; }
        public int CorporationID { get; set; }
        public int? AllianceID { get; set; }
        public float SecurityStatus { get; set; }
        public string Description { get; set; }
    }
}
