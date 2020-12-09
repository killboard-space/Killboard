using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Character
{
    public class PostSSOCharacter
    {
        public long char_id { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int? user_id { get; set; }
    }
}
