using Newtonsoft.Json;
using System;

namespace Killboard.Domain.Models
{
    public partial class CorporationPublicData
    {
        [JsonProperty("alliance_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? AllianceID { get; set; }

        [JsonProperty("ceo_id")]
        public long? CeoID { get; set; }

        [JsonProperty("creator_id")]
        public long? CreatorID { get; set; }

        [JsonProperty("date_founded", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateFounded { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("faction_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? FactionID { get; set; }

        [JsonProperty("home_station_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? HomeStationID { get; set; }

        [JsonProperty("member_count")]
        public int MemberCount { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("shares", NullValueHandling = NullValueHandling.Ignore)]
        public long? Shares { get; set; }

        [JsonProperty("tax_rate")]
        public float TaxRate { get; set; }

        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
    }
}
