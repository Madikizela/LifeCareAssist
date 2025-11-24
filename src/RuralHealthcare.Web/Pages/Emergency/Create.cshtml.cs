using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Emergency;

public class CreateEmergencyModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateEmergencyModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public EmergencyInput Input { get; set; } = new();
    
    public List<Patient> Patients { get; set; } = new();

    public async Task OnGetAsync(Guid? patientId)
    {
        Patients = await _context.Patients
            .OrderBy(p => p.LastName)
            .ToListAsync();

        if (patientId.HasValue)
        {
            Input.PatientId = patientId.Value;
            Input.IsRegisteredPatient = true;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validate based on whether it's a registered patient or anonymous call
        if (Input.IsRegisteredPatient)
        {
            if (!Input.PatientId.HasValue)
            {
                ModelState.AddModelError("", "Please select a patient");
                Patients = await _context.Patients.OrderBy(p => p.LastName).ToListAsync();
                return Page();
            }
        }
        else
        {
            // Anonymous call - require basic information
            if (string.IsNullOrWhiteSpace(Input.CallerName))
            {
                ModelState.AddModelError("Input.CallerName", "Caller name is required");
            }
            if (string.IsNullOrWhiteSpace(Input.CallerPhone))
            {
                ModelState.AddModelError("Input.CallerPhone", "Caller phone is required");
            }
            
            if (!ModelState.IsValid)
            {
                Patients = await _context.Patients.OrderBy(p => p.LastName).ToListAsync();
                return Page();
            }
        }

        Patient? patient = null;
        if (Input.PatientId.HasValue)
        {
            patient = await _context.Patients.FindAsync(Input.PatientId.Value);
        }

        var emergencyCall = new EmergencyCall
        {
            Id = Guid.NewGuid(),
            PatientId = Input.PatientId,
            CallerName = Input.CallerName,
            CallerPhone = Input.CallerPhone,
            CallerIdNumber = Input.CallerIdNumber,
            EmergencyType = Input.EmergencyType,
            Description = Input.Description,
            LocationDescription = Input.LocationDescription,
            CallTime = DateTime.UtcNow,
            Status = "pending",
            Latitude = Input.Latitude ?? patient?.Latitude,
            Longitude = Input.Longitude ?? patient?.Longitude
        };

        _context.EmergencyCalls.Add(emergencyCall);
        await _context.SaveChangesAsync();

        // TODO: Send SMS/notification to emergency services
        // TODO: Notify patient's emergency contact if registered

        TempData["SuccessMessage"] = "Emergency call created successfully! Ambulance is being dispatched.";
        return RedirectToPage("/Emergency/Index");
    }
}

public class EmergencyInput
{
    public bool IsRegisteredPatient { get; set; } = true;
    
    // For registered patients
    public Guid? PatientId { get; set; }
    
    // For anonymous callers
    public string? CallerName { get; set; }
    public string? CallerPhone { get; set; }
    public string? CallerIdNumber { get; set; }
    
    public string EmergencyType { get; set; } = "medical";
    public string? Description { get; set; }
    public string? LocationDescription { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
