using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.User
{
    public class PostUser
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? CharacterID { get; set; }
    }
}
