using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.Stargate;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class StargateRepository : IStargateRepository
    {
        private readonly KillboardContext _ctx;
        public StargateRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetStargate> GetAll() => _ctx.stargates.Select(s => new GetStargate
        {
            DestinationStargateID = s.destination_stargate_id,
            StargateID = s.stargate_id,
            SystemID = s.system_id,
            Name = s.name
        });

        public GetStargate GetStargate(int stargateId) => _ctx.stargates.Where(s => s.stargate_id == stargateId).Select(s => new GetStargate
        {
            DestinationStargateID = s.destination_stargate_id,
            StargateID = s.stargate_id,
            SystemID = s.system_id,
            Name = s.name
        }).FirstOrDefault();
    }
}
