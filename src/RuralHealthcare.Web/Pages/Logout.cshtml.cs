using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RuralHealthcare.Web.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Clear session
        HttpContext.Session.Clear();
        
        // Redirect to home page
        return RedirectToPage("/Index");
    }
}
