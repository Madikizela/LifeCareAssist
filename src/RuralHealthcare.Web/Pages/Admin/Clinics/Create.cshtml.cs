using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Admin.Clinics;

public class CreateClinicModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateClinicModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ClinicInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        // Only system admins can create clinics
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "system_admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        Input.IsActive = true;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Only system admins can create clinics
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "system_admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = Input.Name,
            PhoneNumber = Input.PhoneNumber,
            Address = Input.Address,
            Latitude = Input.Latitude,
            Longitude = Input.Longitude,
            OperatingHours = Input.OperatingHours,
            HasAmbulance = Input.HasAmbulance,
            IsActive = Input.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Admin/Clinics/Index");
    }
}

public class ClinicInput
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? OperatingHours { get; set; }
    public bool HasAmbulance { get; set; }
    public bool IsActive { get; set; } = true;
}
