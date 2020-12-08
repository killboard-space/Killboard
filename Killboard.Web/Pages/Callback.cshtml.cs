using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.Interfaces;
using Killboard.Web.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Killboard.Web.Pages
{
    public class CallbackModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IAPIService _apiService;
        private readonly EsiConfig _esiConfig;

        public CallbackModel(IUserService userService, IAPIService apiService, IOptions<EsiConfig> esiConfigAccessor)
        {
            _userService = userService;
            _apiService = apiService;
            _esiConfig = esiConfigAccessor.Value;
        }

        public async Task<IActionResult> OnGet(string code, string state)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return RedirectToPage("Register", new { ErrorMessage = "Invalid code/state returned from Eve Online SSO." });
            }
            else
            {
                // Adding new Authorized character from Accounts
                if (User.Identity.IsAuthenticated)
                {
                    try
                    {
                        var callbackModel = await _userService.SignInWithSSO(_esiConfig.ClientId, _esiConfig.SecretKey, code);

                        if (callbackModel != null)
                        {
                            var claimValue = User.Claims.Where(a => a.Type == "user_id").Select(a => a.Value).FirstOrDefault();

                            if (int.TryParse(claimValue, out int userId))
                            {
                                // Required to associate new character with appropriate user.
                                callbackModel.user_id = userId;

                                var @char = await _apiService.PostNewCharacter(callbackModel);

                                if (@char != null)
                                {
                                    return RedirectToPage("Accounts", new { Message = $"Successfully added character: {@char.Username} ({@char.CharacterID})" });
                                }
                            }

                        }
                    }
                    catch (ApplicationException aex)
                    {
                        return RedirectToPage("Accounts", new { ErrorMessage = aex.Message });
                    }
                    catch (Exception ex)
                    {
                        return RedirectToPage("Accounts", new { ErrorMessage = ex.Message });
                    }

                    return RedirectToPage("Accounts", new { ErrorMessage = "An error occured while adding new character. Try again later." });
                }
                else // Registration w/ SSO Signup
                {
                    try
                    {
                        var callbackModel = await _userService.SignInWithSSO(_esiConfig.ClientId, _esiConfig.SecretKey, code);

                        if (callbackModel != null)
                        {
                            var @char = await _apiService.PostNewCharacter(callbackModel);

                            if (@char != null)
                            {
                                return RedirectToPage("Register", new { @char.Username, SSO_SIGNUP = true, @char.CharacterID });
                            }
                        }
                    }
                    catch (ApplicationException aex)
                    {
                        return RedirectToPage("Register", new { ErrorMessage = aex.Message });
                    }
                    catch (Exception ex)
                    {
                        return RedirectToPage("Register", new { ErrorMessage = ex.Message });
                    }

                    return RedirectToPage("Register", new { ErrorMessage = "Error while Signing in with SSO/Posting new account, try again later." });
                }
            }
        }
    }
}