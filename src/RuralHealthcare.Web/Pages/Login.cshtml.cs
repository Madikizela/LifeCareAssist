using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public LoginModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet(string? message)
    {
        if (message == "password_changed")
        {
            SuccessMessage = "Password changed successfully. Please login with your new password.";
        }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == Input.Email && u.IsActive);

        if (user == null)
        {
            ErrorMessage = "Invalid email or password";
            return Page();
        }

        // Verify password
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash);
        
        if (!isPasswordValid)
        {
            ErrorMessage = "Invalid email or password";
            return Page();
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Store user info in session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

        // Enforce password change if required
        if (user.RequirePasswordChange)
        {
            return RedirectToPage("/ChangePassword");
        }

        returnUrl ??= user.Role switch
        {
            "system_admin" => "/admin/dashboard",
            "clinic_admin" => "/admin/dashboard",
            "health_worker" => "/patients",
            _ => "/"
        };

        return LocalRedirect(returnUrl);
    }
}

public class LoginInput
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
