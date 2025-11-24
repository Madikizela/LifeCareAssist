using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Patients;

public class EditPatientModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditPatientModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EditPatientInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        var patient = await _context.Patients.FindAsync(id);
        if (patient == null)
        {
            return NotFound();
        }

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId == null || patient.ClinicId != currentUser.ClinicId)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        Input = new EditPatientInput
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            IdNumber = patient.IdNumber,
            DateOfBirth = patient.DateOfBirth,
            PhoneNumber = patient.PhoneNumber,
            AlternativePhoneNumber = patient.AlternativePhoneNumber,
            PreferredLanguage = patient.PreferredLanguage,
            HomeAddress = patient.HomeAddress,
            ChronicConditionsText = string.Join(", ", patient.ChronicConditions),
            AllergiesText = string.Join(", ", patient.Allergies),
            BloodType = patient.BloodType,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone
        };

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
        var patient = await _context.Patients.FindAsync(Input.Id);
        if (patient == null)
        {
            return NotFound();
        }

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId!));
            if (currentUser?.ClinicId == null || patient.ClinicId != currentUser.ClinicId)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        patient.FirstName = Input.FirstName;
        patient.LastName = Input.LastName;
        patient.IdNumber = Input.IdNumber;
        patient.DateOfBirth = Input.DateOfBirth;
        patient.PhoneNumber = Input.PhoneNumber;
        patient.AlternativePhoneNumber = Input.AlternativePhoneNumber;
        patient.PreferredLanguage = Input.PreferredLanguage;
        patient.HomeAddress = Input.HomeAddress;
        patient.BloodType = Input.BloodType;
        patient.EmergencyContactName = Input.EmergencyContactName;
        patient.EmergencyContactPhone = Input.EmergencyContactPhone;
        patient.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(Input.ChronicConditionsText))
        {
            patient.ChronicConditions = Input.ChronicConditionsText
                .Split(',')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();
        }
        else
        {
            patient.ChronicConditions = new List<string>();
        }

        if (!string.IsNullOrWhiteSpace(Input.AllergiesText))
        {
            patient.Allergies = Input.AllergiesText
                .Split(',')
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrEmpty(a))
                .ToList();
        }
        else
        {
            patient.Allergies = new List<string>();
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = patient.Id });
    }
}

public class EditPatientInput
{
    public Guid Id { get; set; }
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
}
