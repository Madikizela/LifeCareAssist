using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Medications;

public class CreateMedicationModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateMedicationModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public MedicationInput Input { get; set; } = new();
    
    public List<Patient> Patients { get; set; } = new();

    public async Task OnGetAsync(Guid? patientId)
    {
        Patients = await _context.Patients.OrderBy(p => p.LastName).ToListAsync();
        
        if (patientId.HasValue)
        {
            Input.PatientId = patientId.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var reminderTimes = Request.Form["reminderTime"]
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => TimeOnly.Parse(t!))
            .ToList();

        if (!reminderTimes.Any())
        {
            ModelState.AddModelError("", "At least one reminder time is required");
            Patients = await _context.Patients.OrderBy(p => p.LastName).ToListAsync();
            return Page();
        }

        var medication = new Medication
        {
            Id = Guid.NewGuid(),
            PatientId = Input.PatientId,
            Name = Input.Name,
            Dosage = Input.Dosage,
            Frequency = Input.Frequency,
            ReminderTimes = reminderTimes,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            Instructions = Input.Instructions,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Patients/Details", new { id = Input.PatientId });
    }
}

public class MedicationInput
{
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public string? Instructions { get; set; }
}
