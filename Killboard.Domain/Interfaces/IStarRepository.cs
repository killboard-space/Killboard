using Killboard.Domain.DTO.Universe.Star;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface IStarRepository
    {
        IEnumerable<GetStar> GetAll();
        GetStar GetStar(int starId);
    }
}
