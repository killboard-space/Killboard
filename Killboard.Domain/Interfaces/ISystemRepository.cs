using Killboard.Domain.DTO.Universe.Stargate;
using Killboard.Domain.DTO.Universe.System;
using Killboard.Domain.Params;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface ISystemRepository
    {
        (IEnumerable<GetSystem> systems, int pages) GetAll(GetAllSystemParameters parameters);
        GetSystem GetSystem(int systemId);
        List<int> GetRoute(int fromSystem, int toSystem);
        List<int> GetSystemsInRange(int fromSystem, int range);
    }
}
