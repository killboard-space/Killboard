using Killboard.Data.Models;
using Killboard.Domain.DTO.Corporation;
using Killboard.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class CorporationRepository : ICorporationRepository
    {
        private readonly KillboardContext _ctx;

        public CorporationRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public IEnumerable<GetCorporation> GetAll() => _ctx.corporations.Select(a => new GetCorporation
        {
            AllianceID = a.alliance_id,
            CorporationID = a.corporation_id,
            Description = a.description,
            Name = a.name,
            Ticker = a.ticker
        });

        public GetCorporation GetCorporation(int corporationId) => _ctx.corporations.Where(a => a.corporation_id == corporationId).Select(a => new GetCorporation
        {
            AllianceID = a.alliance_id,
            CorporationID = a.corporation_id,
            Description = a.description,
            Name = a.name,
            Ticker = a.ticker
        }).FirstOrDefault();

        public GetCorporationDetail GetCorporationDetail(int corporationId) => (from c in _ctx.corporations
                                                                                where c.corporation_id == corporationId
                                                                                join a_temp in _ctx.alliances on c.alliance_id equals a_temp.alliance_id into alliances
                                                                                from a in alliances.DefaultIfEmpty()
                                                                                join c_temp in _ctx.corporations on a.executor_corp_id equals c_temp.corporation_id into exe_corps
                                                                                from exec in exe_corps.DefaultIfEmpty()
                                                                                select new GetCorporationDetail
                                                                                {
                                                                                    Description = c.description,
                                                                                    AllianceID = a.alliance_id,
                                                                                    CorporationID = c.corporation_id,
                                                                                    ExecutorCorpID = exec.corporation_id,
                                                                                    ExecutorCorpName = exec.name,
                                                                                    Name = c.name,
                                                                                    Ticker = c.ticker,
                                                                                    AllianceName = a.name,
                                                                                    AllianceTicker = a.ticker
                                                                                }).FirstOrDefault();
    }
}
