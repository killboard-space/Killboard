using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Belt;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class AsteroidRepository : IAsteroidRepository
    {
        private readonly KillboardContext _ctx;

        public AsteroidRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetAsteroidBelt> GetAll() => _ctx.asteroid_belts.Select(a => new GetAsteroidBelt
        {
            BeltID = a.belt_id,
            Name = a.name,
            PlanetID = a.planet_id
        });

        public GetAsteroidBelt GetAsteroid(int beltId) => _ctx.asteroid_belts.Where(a => a.belt_id == beltId).Select(a => new GetAsteroidBelt
        {
            BeltID = a.belt_id,
            Name = a.name,
            PlanetID = a.planet_id
        }).FirstOrDefault();
    }
}
