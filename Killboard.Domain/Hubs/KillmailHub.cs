using Killboard.Domain.DTO.Killmail;
using Killboard.Domain.Enums;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Killboard.Domain.Hubs
{
    public class KillmailHub : Hub
    {
        private readonly IKillmailService _kmService;

        public KillmailHub(IKillmailService kmService)
        {
            _kmService = kmService;
        }

        public IEnumerable<ListDetail> GetAllKillmails(ListTypes type = ListTypes.ALL, int? filter = null)
        {
            return _kmService.GetAllKillmails(type, filter);
        }
    }
}
