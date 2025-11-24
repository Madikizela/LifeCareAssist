using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Admin.Clinics;

public class EditClinicModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditClinicModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EditClinicInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || (userRole != "system_admin" && userRole != "clinic_admin"))
        {
            return RedirectToPage("/AccessDenied");
        }

        // Clinic admin can only edit their assigned clinic
        if (userRole == "clinic_admin")
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user?.ClinicId != id)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        var clinic = await _context.Clinics.FindAsync(id);
        if (clinic == null)
        {
            return NotFound();
        }

        Input = new EditClinicInput
        {
            Id = clinic.Id,
            Name = clinic.Name,
            PhoneNumber = clinic.PhoneNumber,
            Address = clinic.Address,
            Latitude = clinic.Latitude,
            Longitude = clinic.Longitude,
            OperatingHours = clinic.OperatingHours,
            HasAmbulance = clinic.HasAmbulance,
            IsActive = clinic.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        // Clinic admin can only edit their assigned clinic
        if (userRole == "clinic_admin")
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId!));
            if (user?.ClinicId != Input.Id)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var clinic = await _context.Clinics.FindAsync(Input.Id);
        if (clinic == null)
        {
            return NotFound();
        }

        clinic.Name = Input.Name;
        clinic.PhoneNumber = Input.PhoneNumber;
        clinic.Address = Input.Address;
        clinic.Latitude = Input.Latitude;
        clinic.Longitude = Input.Longitude;
        clinic.OperatingHours = Input.OperatingHours;
        clinic.HasAmbulance = Input.HasAmbulance;
        clinic.IsActive = Input.IsActive;

        await _context.SaveChangesAsync();

        return RedirectToPage("/Admin/Clinics/Index");
    }
}

public class EditClinicInput
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? OperatingHours { get; set; }
    public bool HasAmbulance { get; set; }
    public bool IsActive { get; set; } = true;
}
