using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.DTO.Corporation;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Killboard.Web.Pages
{
    public class CorporationModel : PageModel
    {
        [BindProperty(Name ="corporationId", SupportsGet = true)]
        public int CorporationID { get; set; }

        [BindProperty]
        public GetCorporationDetail CorporationDetail { get; set; }

        private readonly IAPIService _apiService;

        public CorporationModel(IAPIService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (CorporationID == default)
            {
                return RedirectToPage("Index");
            }
            else
            {
                CorporationDetail = await _apiService.GetCorporationDetail(CorporationID);
            }
            return Page();
        }
    }
}