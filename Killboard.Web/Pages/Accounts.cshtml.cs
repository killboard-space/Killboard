using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Data.Models;
using Killboard.Domain.DTO.Character;
using Killboard.Domain.Interfaces;
using Killboard.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Killboard.Web.Pages
{
    public class AccountsModel : PageModel
    {
        [BindProperty]
        public string Message { get; set; }

        [BindProperty]

        public string ErrorMessage { get; set; }

        [BindProperty]
        public List<GetCharacter> Characters { get; set; }

        private readonly IAPIService _apiService;
        private readonly EsiConfig _esiConfig;

        public AccountsModel(IAPIService apiService, IOptions<EsiConfig> esiConfigAccessor)
        {
            _apiService = apiService;
            _esiConfig = esiConfigAccessor.Value;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToPage("Index");

            var claimValue = User.Claims.Where(a => a.Type == "user_id").Select(a => a.Value).FirstOrDefault();

            if (int.TryParse(claimValue, out int userId))
            {
                try
                {
                    Characters = await _apiService.GetCharacters(userId);

                    if (Characters.Count > 0) Message = $"Found {Characters.Count} authorized characters";
                    else Message = "It looks like you have not authorized any characters. You can try adding some below.";

                    ViewData["LoginUrl"] = $"https://login.eveonline.com/v2/oauth/authorize?response_type=code&redirect_uri={_esiConfig.CallbackUrl}&client_id={_esiConfig.ClientId}&scope=publicData%20esi-killmails.read_killmails.v1%20esi-killmails.read_corporation_killmails.v1%20esi-search.search_structures.v1&state=init";
                }
                catch (ApplicationException aex)
                {
                    ErrorMessage = aex.Message;
                    return Page();
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return Page();
                }
            }
            else 
            {
                ErrorMessage = "Failed to get user_id from claims, relogging usually fixes this.";
            }
            return Page();
        }

        public IActionResult CharacterInfo(int characterID)
        {
            return Partial("CharacterInfo", new { CharacterID = characterID });
        }
    }
}