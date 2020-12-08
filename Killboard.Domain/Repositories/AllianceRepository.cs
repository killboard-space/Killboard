using Killboard.Data.Models;
using Killboard.Domain.DTO.Alliance;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class AllianceRepository : IAllianceRepository
    {
        private readonly KillboardContext _ctx;

        public AllianceRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetAlliance> GetAll() => _ctx.alliances.Select(a => new GetAlliance
        {
            AllianceID = a.alliance_id,
            Name = a.name,
            Ticker = a.ticker
        });

        public GetAlliance GetAlliance(int allianceID) => _ctx.alliances.Where(a => a.alliance_id == allianceID).Select(a => new GetAlliance
        {
            AllianceID = a.alliance_id,
            Name = a.name,
            Ticker = a.ticker
        }).FirstOrDefault();

        public GetAllianceDetail GetAllianceDetail(int allianceId) => (from a in _ctx.alliances
                                                                       where a.alliance_id == allianceId
                                                                       join c in _ctx.corporations on a.executor_corp_id equals c.corporation_id
                                                                       select new GetAllianceDetail
                                                                       {
                                                                           AllianceID = a.alliance_id,
                                                                           ExecutorCorpID = a.executor_corp_id,
                                                                           ExecutorCorpName = c.name,
                                                                           Name = a.name,
                                                                           Ticker = a.ticker
                                                                       }).FirstOrDefault();
    }
}
