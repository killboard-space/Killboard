using Killboard.Domain.DTO.Corporation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Killboard.Domain.Interfaces
{
    public interface ICorporationRepository
    {
        IEnumerable<GetCorporation> GetAll();
        GetCorporation GetCorporation(int corporationId);
        GetCorporationDetail GetCorporationDetail(int corporationId);
    }
}
