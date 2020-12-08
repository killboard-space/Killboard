using Killboard.Domain.DTO.Universe.Stargate;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface IStargateRepository
    {
        IEnumerable<GetStargate> GetAll();
        GetStargate GetStargate(int stargateId);
    }
}
