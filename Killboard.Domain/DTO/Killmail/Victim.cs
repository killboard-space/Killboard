using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Killmail
{
    public class Victim
    {
        [JsonProperty("alliance_id")]
        public int? AllianceId { get; set; }

        [JsonProperty("character_id")]
        public long? CharacterId { get; set; }

        [JsonProperty("corporation_id")]
        public int? CorporationId { get; set; }

        [JsonProperty("damage_taken")]
        public long DamageTaken { get; set; }

        [JsonProperty("items")]
        public Item[] Items { get; set; }

        [JsonProperty("position")]
        public Position Position { get; set; }

        [JsonProperty("ship_type_id")]
        public long ShipTypeId { get; set; }
    }
}
