using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Region;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class RegionRepository : IRegionRepository
    {
        private readonly KillboardContext _ctx;
        
        public RegionRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetRegion> GetAll() => _ctx.regions.Select(r => new GetRegion
        {
            RegionID = r.region_id,
            Name = r.name
        });

        public GetRegion GetById(int regionId) => _ctx.regions.Where(r => r.region_id == regionId).Select(r => new GetRegion
        {
            RegionID = r.region_id,
            Name = r.name
        }).FirstOrDefault();
    }
}
