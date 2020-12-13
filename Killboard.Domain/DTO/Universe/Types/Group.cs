using System.Collections.Generic;
using Newtonsoft.Json;

namespace Killboard.Domain.DTO.Universe.Types
{
    public class Group
    {
        [JsonProperty("category_id")]
        public int CategoryId { get; set; }
        [JsonProperty("group_id")]
        public int GroupId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("published")]
        public bool Published { get; set; }
        [JsonProperty("types")]
        public List<int> Types { get; set; }
    }
}
