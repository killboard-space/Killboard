using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Enums;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Killboard.Domain.Hubs
{
    public class KillmailHub : Hub
    {
        private readonly IKillmailService _kmService;

        public KillmailHub(IKillmailService kmService)
        {
            _kmService = kmService;
        }

        public async Task<IEnumerable<ListDetail>> GetAllKillmails(ListTypes type = ListTypes.ALL, int? filter = null)
        {
            return await _kmService.GetAllKillmails(type, filter);
        }
    }
}
