using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Star;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class StarRepository : IStarRepository
    {
        private readonly KillboardContext _ctx;

        public StarRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetStar> GetAll() => _ctx.stars.Select(s => new GetStar
        {
            StarID = s.star_id,
            SystemID = s.system_id,
            Name = s.name,
            TypeID = s.type_id
        });

        public GetStar GetStar(int starId) => _ctx.stars.Where(s => s.star_id == starId).Select(s => new GetStar
        {
            StarID = s.star_id,
            SystemID = s.system_id,
            Name = s.name,
            TypeID = s.type_id
        }).FirstOrDefault();
    }
}
