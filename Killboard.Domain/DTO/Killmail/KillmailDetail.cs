using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Killmail
{
    public class KillmailDetail
    {
        [JsonProperty("attackers")]
        public Attacker[] Attackers { get; set; }

        [JsonProperty("killmail_id")]
        public long KillmailId { get; set; }

        [JsonProperty("killmail_time")]
        public DateTimeOffset KillmailTime { get; set; }

        [JsonProperty("solar_system_id")]
        public long SolarSystemId { get; set; }

        [JsonProperty("victim")]
        public Victim Victim { get; set; }
    }
}
