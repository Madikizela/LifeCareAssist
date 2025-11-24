using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Appointments;

public class CreateAppointmentModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateAppointmentModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public AppointmentInput Input { get; set; } = new();
    
    public List<Patient> Patients { get; set; } = new();
    public List<Clinic> Clinics { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? patientId)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        Patients = await _context.Patients.OrderBy(p => p.LastName).ToListAsync();
        Clinics = await _context.Clinics.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        
        if (patientId.HasValue)
        {
            Input.PatientId = patientId.Value;
        }

        Input.ScheduledDateTime = DateTime.Now.AddDays(1);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Patients = await _context.Patients.OrderBy(p => p.LastName).ToListAsync();
            Clinics = await _context.Clinics.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = Input.PatientId,
            ClinicId = Input.ClinicId,
            ScheduledDateTime = Input.ScheduledDateTime,
            AppointmentType = Input.AppointmentType,
            Status = "scheduled",
            Notes = Input.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // TODO: Send reminder SMS/email to patient

        return RedirectToPage("/Appointments/Index");
    }
}

public class AppointmentInput
{
    public Guid PatientId { get; set; }
    public Guid? ClinicId { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public string AppointmentType { get; set; } = "checkup";
    public string? Notes { get; set; }
}
