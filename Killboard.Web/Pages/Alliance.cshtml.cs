using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.DTO.Alliance;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Killboard.Web.Pages
{
    public class AllianceModel : PageModel
    {
        [BindProperty(Name="allianceId", SupportsGet = true )]
        public int AllianceID { get; set; }

        [BindProperty]
        public GetAllianceDetail AllianceDetail { get; set; }

        private readonly IAPIService _apiService;

        public AllianceModel(IAPIService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (AllianceID == default)
            {
                return RedirectToPage("Index");
            }
            else
            {
                AllianceDetail = await _apiService.GetAllianceDetail(AllianceID);
            }
            return Page();
        }
    }
}