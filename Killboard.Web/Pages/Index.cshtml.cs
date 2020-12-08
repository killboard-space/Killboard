using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Killboard.Web.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Message { get; set; }

        public IndexModel()
        {
        }

        public void OnGet()
        {

        }
    }
}
