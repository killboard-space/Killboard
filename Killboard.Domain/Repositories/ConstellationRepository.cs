using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Constellation;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class ConstellationRepository : IConstellationRepository
    {
        private readonly KillboardContext _ctx;
        public ConstellationRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetConstellation> GetAll() => _ctx.constellations.Select(c => new GetConstellation
        {
            ConstellationID = c.constellation_id,
            Name = c.name,
            RegionID = c.region_id
        });

        public GetConstellation GetConstellation(int constellationId) => _ctx.constellations.Where(c => c.constellation_id == constellationId).Select(c => new GetConstellation
        {
            ConstellationID = c.constellation_id,
            Name = c.name,
            RegionID = c.region_id
        }).FirstOrDefault();
    }
}
