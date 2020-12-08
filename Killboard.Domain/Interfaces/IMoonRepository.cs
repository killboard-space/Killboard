using Killboard.Domain.DTO.Universe.Moon;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface IMoonRepository
    {
        IEnumerable<GetMoon> GetAll();
        GetMoon GetMoon(int moonId);
    }
}
