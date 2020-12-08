using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Planet;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class PlanetRepository : IPlanetRepository
    {
        private readonly KillboardContext _ctx;
        public PlanetRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetPlanet> GetAll() => _ctx.planets.Select(p => new GetPlanet
        {
            PlanetID = p.planet_id,
            SystemID = p.system_id,
            Name = p.name
        });

        public GetPlanet GetPlanet(int planetId) => _ctx.planets.Where(p => p.planet_id == planetId).Select(p => new GetPlanet
        {
            PlanetID = p.planet_id,
            SystemID = p.system_id,
            Name = p.name
        }).FirstOrDefault();
    }
}
