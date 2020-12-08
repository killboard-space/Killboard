using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Killboard.Web
{
    public class ProfileModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public string Message { get; set; }

        private readonly IAPIService _apiService;

        public ProfileModel(IAPIService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            else
            {
                var claimValue = User.Claims.Where(a => a.Type == "user_id").Select(a => a.Value).FirstOrDefault();

                if(int.TryParse(claimValue, out int userId))
                {
                    try
                    {
                        Email = await _apiService.GetEmail(userId);
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
            }
            return Page();
        }
    }
}