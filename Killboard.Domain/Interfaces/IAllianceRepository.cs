using Killboard.Domain.DTO.Alliance;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface IAllianceRepository
    {
        IEnumerable<GetAlliance> GetAll();
        GetAlliance GetAlliance(int allianceID);
        GetAllianceDetail GetAllianceDetail(int allianceId);
    }
}
