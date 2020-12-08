using Newtonsoft.Json;
using System;

namespace Killboard.Domain.Models
{
    public partial class AlliancePublicData
    {
        [JsonProperty("creator_corporation_id")]
        public int CreatorCorporationID { get; set; }

        [JsonProperty("creator_id")]
        public int CreatorID { get; set; }

        [JsonProperty("date_founded")]
        public DateTime DateFounded { get; set; }

        [JsonProperty("executor_corporation_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? ExecutorCorporationID { get; set; }

        [JsonProperty("faction_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? FactionID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ticker")]
        public string Ticker { get; set; }
    }
}
