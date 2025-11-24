using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages;

public class ChangePasswordModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ChangePasswordModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PasswordChangeInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public bool RequirePasswordChange { get; set; }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        RequirePasswordChange = true;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.NewPassword != Input.ConfirmPassword)
        {
            ErrorMessage = "New password and confirmation do not match";
            return Page();
        }

        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        if (user == null)
        {
            return RedirectToPage("/Login");
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(Input.CurrentPassword, user.PasswordHash))
        {
            ErrorMessage = "Current password is incorrect";
            return Page();
        }

        // Validate new password strength
        if (!IsPasswordStrong(Input.NewPassword))
        {
            ErrorMessage = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character";
            return Page();
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
        user.RequirePasswordChange = false;
        await _context.SaveChangesAsync();

        return RedirectToPage("/Login", new { message = "password_changed" });
    }

    private bool IsPasswordStrong(string password)
    {
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) return false;
        return true;
    }
}

public class PasswordChangeInput
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
