using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ResetPasswordModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ResetPasswordInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string token)
    {
        await _context.Database.MigrateAsync();
        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS PasswordResetTokens (
                Id TEXT NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
                UserId TEXT NOT NULL,
                Token TEXT NOT NULL,
                ExpiresAt TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UsedAt TEXT NULL,
                CONSTRAINT FK_PasswordResetTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
            );");
        }
        catch { }

        if (string.IsNullOrEmpty(token))
        {
            ErrorMessage = "Invalid reset link.";
            return Page();
        }

        var reset = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (reset == null || reset.UsedAt != null || reset.ExpiresAt < DateTime.UtcNow)
        {
            ErrorMessage = "This reset link is invalid or has expired.";
            return Page();
        }

        Input.Token = token;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _context.Database.MigrateAsync();
        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"CREATE TABLE IF NOT EXISTS PasswordResetTokens (
                Id TEXT NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
                UserId TEXT NOT NULL,
                Token TEXT NOT NULL,
                ExpiresAt TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UsedAt TEXT NULL,
                CONSTRAINT FK_PasswordResetTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
            );
            CREATE UNIQUE INDEX IF NOT EXISTS IX_PasswordResetTokens_Token ON PasswordResetTokens (Token);");
        }
        catch { }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var reset = await _context.PasswordResetTokens.FirstOrDefaultAsync(t => t.Token == Input.Token);
        if (reset == null || reset.UsedAt != null || reset.ExpiresAt < DateTime.UtcNow)
        {
            ErrorMessage = "This reset link is invalid or has expired.";
            return Page();
        }

        var user = await _context.Users.FindAsync(reset.UserId);
        if (user == null)
        {
            ErrorMessage = "User not found.";
            return Page();
        }

        if (Input.NewPassword != Input.ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        if (Input.NewPassword.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return Page();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.NewPassword);
        user.RequirePasswordChange = false;
        reset.UsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        SuccessMessage = "Your password has been updated. You can now sign in.";
        return Page();
    }
}

public class ResetPasswordInput
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
