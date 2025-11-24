using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public HomeLoginInput Input { get; set; } = new();

    public int TotalPatients { get; set; }
    public int ActiveClinics { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        TotalPatients = await _context.Patients.CountAsync();
        ActiveClinics = await _context.Clinics.CountAsync(c => c.IsActive);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == Input.Email && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
        {
            ErrorMessage = "Invalid email or password";
            await OnGetAsync();
            return Page();
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Store in session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

        // Redirect based on role
        var returnUrl = user.Role switch
        {
            "system_admin" => "/admin/dashboard",
            "clinic_admin" => "/admin/dashboard",
            "health_worker" => "/patients",
            _ => "/patients"
        };

        return LocalRedirect(returnUrl);
    }
}

public class HomeLoginInput
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
