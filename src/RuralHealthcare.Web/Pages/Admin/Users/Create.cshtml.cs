using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;
using RuralHealthcare.Services;
using RuralHealthcare.Services.Interfaces;

namespace RuralHealthcare.Web.Pages.Admin.Users;

public class CreateUserModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public CreateUserModel(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [BindProperty]
    public UserInput Input { get; set; } = new();
    
    public List<Clinic> Clinics { get; set; } = new();
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        Clinics = await _context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUserId = HttpContext.Session.GetString("UserId");
        var currentUserRole = HttpContext.Session.GetString("UserRole");

        if (!ModelState.IsValid)
        {
            await LoadClinicsAsync(currentUserId!, currentUserRole!);
            return Page();
        }

        // Clinic admin restrictions
        if (currentUserRole == "clinic_admin")
        {
            // Can only create health_worker or caregiver
            if (Input.Role == "system_admin" || Input.Role == "clinic_admin")
            {
                ModelState.AddModelError("", "Clinic admins can only create Health Workers and Caregivers");
                await LoadClinicsAsync(currentUserId!, currentUserRole);
                return Page();
            }

            // Must assign to their clinic
            var currentUser = await _context.Users.FindAsync(Guid.Parse(currentUserId!));
            if (currentUser?.ClinicId.HasValue == true)
            {
                Input.ClinicId = currentUser.ClinicId.Value;
            }
        }

        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Input.Email", "A user with this email already exists");
            await LoadClinicsAsync(currentUserId!, currentUserRole!);
            return Page();
        }

        // Generate secure password
        var temporaryPassword = PasswordGenerator.GenerateSecurePassword(12);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = Input.Email,
            PasswordHash = passwordHash,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            Role = Input.Role,
            PhoneNumber = Input.PhoneNumber,
            ClinicId = Input.ClinicId,
            IsActive = true,
            RequirePasswordChange = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send welcome email with credentials
        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, temporaryPassword);
            SuccessMessage = $"User created successfully! Welcome email sent to {user.Email}";
        }
        catch (Exception ex)
        {
            // Log error but don't fail the user creation
            Console.WriteLine($"Failed to send email: {ex.Message}");
            SuccessMessage = $"User created successfully! Please manually provide credentials to {user.Email}. Temporary password: {temporaryPassword}";
        }

        TempData["SuccessMessage"] = SuccessMessage;
        return RedirectToPage("/Admin/Users/Index");
    }

    private async Task LoadClinicsAsync(string userId, string userRole)
    {
        if (userRole == "clinic_admin")
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user?.ClinicId.HasValue == true)
            {
                Clinics = await _context.Clinics
                    .Where(c => c.Id == user.ClinicId.Value && c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
        }
        else
        {
            Clinics = await _context.Clinics
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}

public class UserInput
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? ClinicId { get; set; }
}
