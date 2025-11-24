using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Admin.Users;

public class EditUserModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditUserModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public UserEditInput Input { get; set; } = new();
    
    public List<Clinic> Clinics { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var currentUserRole = HttpContext.Session.GetString("UserRole");
        if (currentUserRole != "system_admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        Input = new UserEditInput
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            ClinicId = user.ClinicId,
            IsActive = user.IsActive
        };

        Clinics = await _context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentUserRole = HttpContext.Session.GetString("UserRole");
        if (currentUserRole != "system_admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        if (!ModelState.IsValid)
        {
            Clinics = await _context.Clinics.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        var user = await _context.Users.FindAsync(Input.Id);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;
        user.Email = Input.Email;
        user.PhoneNumber = Input.PhoneNumber;
        user.Role = Input.Role;
        user.ClinicId = Input.ClinicId;
        user.IsActive = Input.IsActive;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "User updated successfully!";
        return RedirectToPage("/Admin/Users/Index");
    }
}

public class UserEditInput
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? ClinicId { get; set; }
    public bool IsActive { get; set; }
}
