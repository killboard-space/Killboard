using Killboard.Domain.DTO.Universe.Planet;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface IPlanetRepository
    {
        IEnumerable<GetPlanet> GetAll();
        GetPlanet GetPlanet(int planetId);
    }
}
