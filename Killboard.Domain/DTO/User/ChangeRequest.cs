using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.User
{
    public class ChangeRequest
    {
        public string Key { get; set; }
        public string Password { get; set; }
    }
}
