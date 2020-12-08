using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Killmail
{
    public class Item
    {
        [JsonProperty("flag")]
        public int Flag { get; set; }

        [JsonProperty("item_type_id")]
        public int ItemTypeId { get; set; }

        [JsonProperty("quantity_destroyed", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuantityDestroyed { get; set; }

        [JsonProperty("singleton")]
        public int Singleton { get; set; }

        [JsonProperty("quantity_dropped", NullValueHandling = NullValueHandling.Ignore)]
        public int? QuantityDropped { get; set; }
    }
}
