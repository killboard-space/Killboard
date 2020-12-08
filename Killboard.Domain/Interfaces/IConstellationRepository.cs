using Killboard.Domain.DTO.Universe.Constellation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface IConstellationRepository
    {
        IEnumerable<GetConstellation> GetAll();
        GetConstellation GetConstellation(int constellationId);
    }
}
