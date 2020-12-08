using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.User
{
    public class PostAuthenticateUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
