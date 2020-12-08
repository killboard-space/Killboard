using Newtonsoft.Json;

namespace Killboard.Domain.Models
{
    public class KillmailModel
    {
        [JsonProperty("killmail_id")]
        public int killmail_id { get; set; }

        [JsonProperty("killmail_hash")]
        public string killmail_hash { get; set; }
    }
}
