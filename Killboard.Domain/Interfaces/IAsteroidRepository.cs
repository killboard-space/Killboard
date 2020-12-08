using Killboard.Domain.DTO.Universe.Belt;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface IAsteroidRepository
    {
        IEnumerable<GetAsteroidBelt> GetAll();
        GetAsteroidBelt GetAsteroid(int beltId);
    }
}
