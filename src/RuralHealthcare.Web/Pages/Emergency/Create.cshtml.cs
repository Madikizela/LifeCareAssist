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
    public Clinic? NearestAmbulanceClinic { get; set; }
    public string? NearestAmbulanceDistanceLabel { get; set; }
    public List<Clinic> AmbulanceClinics { get; set; } = new();

    public async Task OnGetAsync(Guid? patientId)
    {
        AmbulanceClinics = await _context.Clinics.Where(c => c.IsActive && c.HasAmbulance).ToListAsync();
        Patients = await _context.Patients
            .OrderBy(p => p.LastName)
            .ToListAsync();

        if (patientId.HasValue)
        {
            Input.PatientId = patientId.Value;
            Input.IsRegisteredPatient = true;
            var patient = await _context.Patients.FindAsync(patientId.Value);
            if (patient != null && patient.Latitude.HasValue && patient.Longitude.HasValue)
            {
                if (AmbulanceClinics.Any())
                {
                    var best = AmbulanceClinics
                        .Select(c => new { C = c, D = Haversine(patient.Latitude.Value, patient.Longitude.Value, c.Latitude, c.Longitude) })
                        .OrderBy(x => x.D)
                        .First();
                    NearestAmbulanceClinic = best.C;
                    NearestAmbulanceDistanceLabel = Math.Round(best.D, 1) + " km";
                }
            }
        }
        else
        {
            var qlatStr = Request.Query["lat"].FirstOrDefault();
            var qlngStr = Request.Query["lng"].FirstOrDefault();
            if (double.TryParse(qlatStr, out var qlat) && double.TryParse(qlngStr, out var qlng))
            {
                if (AmbulanceClinics.Any())
                {
                    var best = AmbulanceClinics
                        .Select(c => new { C = c, D = Haversine(qlat, qlng, c.Latitude, c.Longitude) })
                        .OrderBy(x => x.D)
                        .First();
                    NearestAmbulanceClinic = best.C;
                    NearestAmbulanceDistanceLabel = Math.Round(best.D, 1) + " km";
                }
            }
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

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
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
