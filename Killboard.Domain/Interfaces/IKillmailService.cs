using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Killboard.Domain.Services;
using System.Threading.Tasks;

namespace Killboard.Domain.Interfaces
{
    public interface IKillmailService : IDisposable
    {
        Task<IEnumerable<ListDetail>> GetAllKillmails(ListTypes type = ListTypes.ALL, int? filter = null);
    }
}
