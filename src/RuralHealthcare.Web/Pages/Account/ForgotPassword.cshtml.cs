using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;
using RuralHealthcare.Services.Interfaces;

namespace RuralHealthcare.Web.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordModel(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [BindProperty]
    public ForgotPasswordInput Input { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Ensure latest migrations are applied (guards against missing tables in dev)
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
            await _context.Database.ExecuteSqlRawAsync(@"CREATE UNIQUE INDEX IF NOT EXISTS IX_PasswordResetTokens_Token ON PasswordResetTokens (Token);");
        }
        catch { }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == Input.Email);
        if (user == null)
        {
            ErrorMessage = "This email is not registered.";
            return Page();
        }

        var token = Guid.NewGuid().ToString("N");
        var reset = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        _context.PasswordResetTokens.Add(reset);
        await _context.SaveChangesAsync();

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);
            SuccessMessage = "A password reset link has been sent to your email.";
            return Page();
        }
        catch
        {
            ErrorMessage = "Failed to send reset email. Please try again later.";
            return Page();
        }
    }
}

public class ForgotPasswordInput
{
    public string Email { get; set; } = string.Empty;
}
