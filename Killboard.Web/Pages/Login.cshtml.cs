using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Killboard.Domain.DTO.User;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Killboard.Web
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [FromQuery]
        public string Message { get; set; }

        [FromQuery]
        public string ErrorMessage { get; set; }

        private readonly IAPIService _apiService;

        public LoginModel(IAPIService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated) return RedirectToPage("Index");

            if (!string.IsNullOrEmpty(Request.Query["Username"]))
            {
                Username = Request.Query["Username"];
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                var authenticated = await _apiService.PostAuthenticate(new PostAuthenticateUser
                {
                    Username = Username,
                    Password = Password
                });

                if (authenticated != null)
                {
                    var claims = new List<Claim>()
                    {
                        new Claim("username", authenticated.username),
                        new Claim("user_id", authenticated.user_id.ToString())
                    };

                    ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");

                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                        IsPersistent = true,
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProperties);

                    return RedirectToPage("Index");
                }
                else
                {
                    ErrorMessage = "Incorrect Username/Password combination";
                }
            }
            catch(ApplicationException aex)
            {
                ErrorMessage = aex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            return Page();
        }
    }
}