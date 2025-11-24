using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Emergency;

public class EmergencyIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EmergencyIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<EmergencyCall> EmergencyCalls { get; set; } = new();
    public string? CurrentUserRole { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        CurrentUserRole = userRole;

        var query = _context.EmergencyCalls
            .Include(e => e.Patient)
            .Where(e => e.Status != "completed" && e.Status != "cancelled")
            .AsQueryable();

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId != null)
            {
                var cid = currentUser.ClinicId.Value;
                query = query.Where(e => e.PatientId != null && e.Patient!.ClinicId == cid);
            }
            else
            {
                query = query.Where(e => false);
            }
        }

        EmergencyCalls = await query
            .OrderByDescending(e => e.CallTime)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostDispatchAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        // Check permissions: system_admin, clinic_admin, or health_worker can dispatch
        if (userRole != "system_admin" && userRole != "clinic_admin" && userRole != "health_worker")
        {
            TempData["ErrorMessage"] = "You don't have permission to dispatch ambulances.";
            return RedirectToPage();
        }

        var emergency = await _context.EmergencyCalls
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (emergency == null)
        {
            return NotFound();
        }

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId == null || emergency.Patient?.ClinicId != currentUser.ClinicId)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        if (emergency.Status != "pending")
        {
            TempData["ErrorMessage"] = "This emergency call has already been dispatched.";
            return RedirectToPage();
        }

        emergency.Status = "dispatched";
        emergency.DispatchedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Ambulance dispatched successfully!";
        
        // TODO: Send SMS notification to caller
        // TODO: Notify ambulance driver
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkArrivedAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        var userRole = HttpContext.Session.GetString("UserRole");
        var emergency = await _context.EmergencyCalls
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (emergency == null)
        {
            return NotFound();
        }

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId == null || emergency.Patient?.ClinicId != currentUser.ClinicId)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        emergency.Status = "arrived";
        emergency.ArrivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Marked as arrived on scene.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        var userRole = HttpContext.Session.GetString("UserRole");
        var emergency = await _context.EmergencyCalls
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (emergency == null)
        {
            return NotFound();
        }

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId == null || emergency.Patient?.ClinicId != currentUser.ClinicId)
            {
                return RedirectToPage("/AccessDenied");
            }
        }

        emergency.Status = "completed";
        emergency.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Emergency call completed.";
        return RedirectToPage();
    }
}
