using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.DTO.Universe.Star
{
    public class GetStar
    {
        public int StarID { get; set; }
        public string Name { get; set; }
        public int SystemID { get; set; }
        public int TypeID { get; set; }
    }
}
