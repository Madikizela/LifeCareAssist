using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Appointments;

public class AppointmentsIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AppointmentsIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Appointment> Appointments { get; set; } = new();
    public string? Filter { get; set; }

    public async Task<IActionResult> OnGetAsync(string? filter)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        Filter = filter;

        var query = _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Clinic)
            .AsQueryable();

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId != null)
            {
                var cid = currentUser.ClinicId.Value;
                query = query.Where(a => a.ClinicId == cid);
            }
            else
            {
                query = query.Where(a => false);
            }
        }

        if (filter == "upcoming")
        {
            query = query.Where(a => a.ScheduledDateTime >= DateTime.Now && a.Status == "scheduled");
        }
        else if (filter == "today")
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            query = query.Where(a => a.ScheduledDateTime >= today && a.ScheduledDateTime < tomorrow);
        }
        else if (filter == "missed")
        {
            query = query.Where(a => a.Status == "missed");
        }
        else if (filter == "completed")
        {
            query = query.Where(a => a.Status == "completed");
        }

        Appointments = await query
            .OrderBy(a => a.ScheduledDateTime)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostMarkCompletedAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            if (userRole == "clinic_admin")
            {
                var currentUser = await _context.Users.FindAsync(Guid.Parse(userId!));
                if (currentUser?.ClinicId == null || appointment.ClinicId != currentUser.ClinicId)
                {
                    return RedirectToPage("/AccessDenied");
                }
            }
            appointment.Status = "completed";
            appointment.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkMissedAsync(Guid id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            if (userRole == "clinic_admin")
            {
                var currentUser = await _context.Users.FindAsync(Guid.Parse(userId!));
                if (currentUser?.ClinicId == null || appointment.ClinicId != currentUser.ClinicId)
                {
                    return RedirectToPage("/AccessDenied");
                }
            }
            appointment.Status = "missed";
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}
