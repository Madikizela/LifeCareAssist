using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Account;

public class ChangePasswordModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ChangePasswordModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ChangePasswordInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        if (user == null)
        {
            return RedirectToPage("/Index");
        }

        if (!BCrypt.Net.BCrypt.Verify(Input.CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("", "Current password is incorrect.");
            return Page();
        }

        if (Input.NewPassword != Input.ConfirmPassword)
        {
            ModelState.AddModelError("", "New password and confirmation do not match.");
            return Page();
        }

        if (Input.NewPassword.Length < 8)
        {
            ModelState.AddModelError("", "Password must be at least 8 characters.");
            return Page();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
        user.RequirePasswordChange = false;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Password updated successfully.";
        return RedirectToPage("/Index");
    }
}

public class ChangePasswordInput
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

