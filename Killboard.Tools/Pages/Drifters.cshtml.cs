using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.DTO.Universe.System;
using Killboard.Tools.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Killboard.Tools
{
    public class DriftersModel : PageModel
    {
        [FromQuery(Name = "amount")]
        public string Amount { get; set; }

        [FromQuery(Name = "region")]
        public string Region { get; set; }

        [FromQuery(Name = "security")]
        public string Security { get; set; }

        [BindProperty]
        public List<GetSystem> AllSystems { get; set; }

        private readonly IKillboardAPIService _apiService;
        private readonly IMemoryCache _cache;

        public DriftersModel(IKillboardAPIService apiService, IMemoryCache cache)
        {
            _apiService = apiService;
            _cache = cache;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!string.IsNullOrEmpty(Amount))
            {
                if (!Amount.Equals("All", StringComparison.CurrentCultureIgnoreCase)
                    && !Amount.Equals("10", StringComparison.CurrentCultureIgnoreCase)
                    && !Amount.Equals("25", StringComparison.CurrentCultureIgnoreCase)
                    && !Amount.Equals("50", StringComparison.CurrentCultureIgnoreCase))
                {
                    Amount = null;
                }
            }

            if (!string.IsNullOrEmpty(Security))
            {
                if (!Security.Equals("All", StringComparison.CurrentCultureIgnoreCase)
                    && !Security.Equals("High", StringComparison.CurrentCultureIgnoreCase)
                    && !Security.Equals("Low", StringComparison.CurrentCultureIgnoreCase)
                    && !Security.Equals("Null", StringComparison.CurrentCultureIgnoreCase))
                {
                    Security = null;
                }
            }

            AllSystems = await _cache.GetOrCreateAsync("AllSystems", async entry => 
            {
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                return await _apiService.GetAllSystems();
            });

            return Page();
        }

        public IActionResult OnGetSearch(string term)
        {
            AllSystems = (List<GetSystem>)_cache.Get("AllSystems");
            var systems = AllSystems.Where(s => s.Name.ToLower().StartsWith(term.ToLower().Trim())).Select(s => s.Name);
            return new JsonResult(systems);
        }

        public async Task<IActionResult> OnGetRangeSearch(string systemName, int jumps)
        {
            AllSystems = (List<GetSystem>)_cache.Get("AllSystems");
            int fromSystemId = AllSystems.Where(s => s.Name.ToLower().Equals(systemName.ToLower().Trim())).Select(s => s.SystemID).FirstOrDefault();
            if(fromSystemId == default)
            {
                // Invalid SystemName
                return BadRequest("Invalid System Name - No Match");
            }
            else if(jumps < 1)
            {
                // Invalid Jump Range
                return BadRequest("Invalid Jump Range - Must be > 1");
            }
            else
            {
                var systemsInRange = await _cache.GetOrCreateAsync(new { fromSystemId, jumps }, async entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                    entry.SetSlidingExpiration(TimeSpan.FromMinutes(15));
                    return await _apiService.GetSystemsWithinRange(fromSystemId, jumps);
                });
;
                return new JsonResult(systemsInRange);
            }
        }
    }
}