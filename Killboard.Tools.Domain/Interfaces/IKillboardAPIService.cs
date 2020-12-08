using Killboard.Domain.DTO.Universe.System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Killboard.Tools.Domain.Interfaces
{
    public interface IKillboardAPIService
    {
        Task<List<GetSystem>> GetAllSystems();
        Task<int[]> GetSystemsWithinRange(int systemId, int jumps);
    }
}
