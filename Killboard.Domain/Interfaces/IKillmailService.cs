using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Killboard.Domain.Services;

namespace Killboard.Domain.Interfaces
{
    public interface IKillmailService
    {
        List<ListDetail> GetAllKillmails(ListTypes type = ListTypes.ALL, int? filter = null);
    }
}
