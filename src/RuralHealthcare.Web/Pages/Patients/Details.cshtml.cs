using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Patients;

public class PatientDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public PatientDetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Patient Patient { get; set; } = null!;
    public List<Medication> Medications { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        var patient = await _context.Patients
            .Include(p => p.Medications.Where(m => m.IsActive))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            return NotFound();
        }

        // Restrict clinic admin to patients in their clinic only
        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            var cid = currentUser?.ClinicId;
            if (cid == null || patient.ClinicId != cid)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        Patient = patient;
        Medications = patient.Medications.ToList();

        return Page();
    }
}
