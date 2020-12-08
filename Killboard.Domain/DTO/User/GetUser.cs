using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.User
{
    public class GetUser
    {
        public int user_id { get; set; }
        public string username { get; set; }
    }
}
