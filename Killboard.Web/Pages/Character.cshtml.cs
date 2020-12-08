using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.DTO.Character;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Killboard.Web.Pages
{
    public class CharacterModel : PageModel
    {
        [BindProperty(Name = "characterId", SupportsGet = true)]
        public int CharacterID { get; set; }

        [BindProperty]
        public GetCharacterDetail CharacterDetail { get; set; }

        private readonly IAPIService _apiService;

        public CharacterModel(IAPIService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (CharacterID == default)
            {
                return RedirectToPage("Index");
            }
            else
            {
                try
                {
                    CharacterDetail = await _apiService.GetCharacterDetail(CharacterID);
                }
                catch (ApplicationException)
                {
                    return RedirectToPage("Index");
                }
                catch (Exception)
                {
                    return RedirectToPage("Index");
                }
            }
            return Page();
        }
    }
}