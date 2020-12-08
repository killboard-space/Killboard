using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.DTO.User;
using Killboard.Domain.Interfaces;
using Killboard.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Killboard.Web
{
    public class RegisterModel : PageModel
    {
        [FromQuery]
        public string Username { get; set; }

        [BindProperty]
        public string Email { get; set; }
        
        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [FromQuery]
        public string ErrorMessage { get; set; }

        [FromQuery]
        public bool SSO_SIGNUP { get; set; }

        [FromQuery]
        public int CharacterID { get; set; }

        private readonly IAPIService _apiService;
        private readonly EsiConfig _esiConfig;

        public RegisterModel(IAPIService apiService, IOptions<EsiConfig> esiConfigAccessor)
        {
            _apiService = apiService;
            _esiConfig = esiConfigAccessor.Value;
        }

        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("Index");
                
            ViewData["LoginUrl"] = $"https://login.eveonline.com/v2/oauth/authorize?response_type=code&redirect_uri={_esiConfig.CallbackUrl}&client_id={_esiConfig.ClientId}&scope=publicData%20esi-killmails.read_killmails.v1%20esi-killmails.read_corporation_killmails.v1%20esi-search.search_structures.v1&state=init";
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("Password", "Passwords do not match!");
                return Page();
            }
            else
            {
                try
                {
                    GetUser user;
                    // With character Association
                    if (CharacterID != default)
                    {
                        user = await _apiService.PostNewUser(new PostUser
                        {
                            CharacterID = CharacterID,
                            Email = Email,
                            Password = Password,
                            Username = Username
                        });
                    }
                    else // Without associated character
                    {
                        user = await _apiService.PostNewUser(new PostUser
                        {
                            Email = Email,
                            Password = Password,
                            Username = Username
                        });
                    }

                    if(user != null)
                    {
                        return RedirectToPage("Login", new { Message = "You have successfully registered, please login.", Username = user.username, CharacterID = CharacterID != default ? (int?) CharacterID : null });
                    }
                    else
                    {
                        ErrorMessage = "Error whilst posting new account, please try again later.";
                        return Page();
                    }
                }
                catch (ApplicationException aex)
                {
                    ErrorMessage = aex.Message;
                    return Page();
                }
                catch(Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return Page();
                }
            }
        }
    }
}