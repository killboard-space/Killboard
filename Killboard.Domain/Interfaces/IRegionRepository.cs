using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Region;
using System.Collections.Generic;

namespace Killboard.Domain.Interfaces
{
    public interface IRegionRepository
    {
        IEnumerable<GetRegion> GetAll();
        GetRegion GetById(int regionId);
    }
}
