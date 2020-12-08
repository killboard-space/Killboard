using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Killboard.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Killboard.Web.Pages
{
    public class ForgetModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public string Message { get; set; }

        [BindProperty]
        public string ErrorMessage { get; set; }

        [BindProperty]
        public bool IsWithValidKey { get; set; }

        [FromQuery]
        public string Key { get; set; }

        private readonly IAPIService _apiService;

        public ForgetModel(IAPIService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (User.Identity.IsAuthenticated) 
            { 
                return RedirectToPage("Index");
            }
            else if (!string.IsNullOrEmpty(Key))
            {
                try
                {
                    IsWithValidKey = await _apiService.ValidateKey(Key);

                    if (!IsWithValidKey) ErrorMessage = "Invalid Key provided. This usually means it is expired or does not exist.";
                }
                catch (ApplicationException aex)
                {
                    ErrorMessage = aex.Message;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (IsWithValidKey)
            {
                if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(ConfirmPassword))
                {
                    try
                    {
                        bool success = await _apiService.ChangePassword(Key, Password);

                        if (success) Message = "Password reset successful! Click <a href='~/login'>Here</a> to login.";
                        else ErrorMessage = "There was an error while changing your password, please try again later.";
                    }
                    catch (ApplicationException aex)
                    {
                        ErrorMessage = aex.Message;
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                }
                else
                {
                    ErrorMessage = "Invalid New Password/Confirm Password";
                }
            }
            else if (!string.IsNullOrEmpty(Email))
            {
                try
                {
                    bool success = await _apiService.SendForgetPasswordRequest(Email);

                    if (success) Message = "If found, a reset link has been sent to the email provided.";
                    else ErrorMessage = "There was an error processing your request, please try again later.";
                }
                catch (ApplicationException aex)
                {
                    ErrorMessage = aex.Message;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }
            }

            return Page();
        }
    }
}