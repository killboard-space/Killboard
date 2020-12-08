using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Moon;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class MoonRepository : IMoonRepository
    {
        private readonly KillboardContext _ctx;

        public MoonRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetMoon> GetAll() => _ctx.moons.Select(m => new GetMoon
        {
            MoonID = m.moon_id,
            PlanetID = m.planet_id,
            Name = m.name
        });

        public GetMoon GetMoon(int moonId) => _ctx.moons.Where(m => m.moon_id == moonId).Select(m => new GetMoon
        {
            MoonID = m.moon_id,
            PlanetID = m.planet_id,
            Name = m.name
        }).FirstOrDefault();
    }
}
