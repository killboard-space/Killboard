using System.Collections.Generic;
using Newtonsoft.Json;

namespace Killboard.Domain.DTO.Universe.Types
{
    public class Category
    {
        [JsonProperty("category_id")]
        public int CategoryId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("published")]
        public bool Published { get; set; }
        [JsonProperty("groups")]
        public List<int> Groups { get; set; }
    }
}
