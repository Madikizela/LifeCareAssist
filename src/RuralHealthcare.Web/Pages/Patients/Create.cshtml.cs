using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Patients;

public class CreatePatientModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreatePatientModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public PatientInput Input { get; set; } = new();

    public List<Clinic> Clinics { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        Clinics = await _context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            IdNumber = Input.IdNumber,
            FirstName = Input.FirstName,
            LastName = Input.LastName,
            DateOfBirth = Input.DateOfBirth,
            PhoneNumber = Input.PhoneNumber,
            AlternativePhoneNumber = Input.AlternativePhoneNumber,
            PreferredLanguage = Input.PreferredLanguage,
            HomeAddress = Input.HomeAddress,
            BloodType = Input.BloodType,
            EmergencyContactName = Input.EmergencyContactName,
            EmergencyContactPhone = Input.EmergencyContactPhone,
            CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(Input.ChronicConditionsText))
        {
            patient.ChronicConditions = Input.ChronicConditionsText
                .Split(',')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(Input.AllergiesText))
        {
            patient.Allergies = Input.AllergiesText
                .Split(',')
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrEmpty(a))
                .ToList();
        }
        
        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId != null)
            {
                patient.ClinicId = currentUser.ClinicId;
            }
        }
        else if (Input.ClinicId.HasValue)
        {
            patient.ClinicId = Input.ClinicId;
        }

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = patient.Id });
    }
}

public class PatientInput
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternativePhoneNumber { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public string? HomeAddress { get; set; }
    public string? ChronicConditionsText { get; set; }
    public string? AllergiesText { get; set; }
    public string? BloodType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public Guid? ClinicId { get; set; }
}
