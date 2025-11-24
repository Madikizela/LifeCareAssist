using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.HealthWorker;

public class HealthWorkerDashboardModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public HealthWorkerDashboardModel(ApplicationDbContext context) { _context = context; }

    public int ClinicPatients { get; set; }
    public int TodayAppointmentsCount { get; set; }
    public int ActiveEmergenciesCount { get; set; }
    public int MissedAppointmentsWeek { get; set; }

    public List<Appointment> TodayAppointments { get; set; } = new();
    public List<EmergencyCall> ActiveEmergencies { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId)) { return RedirectToPage("/Index"); }
        if (userRole != "health_worker") { return RedirectToPage("/AccessDenied"); }

        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        var cid = user?.ClinicId;

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        int dayOfWeek = (int)today.DayOfWeek; // Sunday=0, Monday=1
        int offsetToMonday = dayOfWeek == 0 ? -6 : 1 - dayOfWeek;
        var weekStart = today.AddDays(offsetToMonday);
        var weekEnd = weekStart.AddDays(7);

        if (cid.HasValue)
        {
            ClinicPatients = await _context.Patients.CountAsync(p => p.ClinicId == cid.Value);

            TodayAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.ClinicId == cid.Value && a.ScheduledDateTime >= today && a.ScheduledDateTime < tomorrow)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync();
            TodayAppointmentsCount = TodayAppointments.Count;

            ActiveEmergencies = await _context.EmergencyCalls
                .Include(e => e.Patient)
                .Where(e => (e.Status == "pending" || e.Status == "dispatched") && (e.PatientId == null || e.Patient!.ClinicId == cid.Value))
                .OrderByDescending(e => e.CallTime)
                .ToListAsync();
            ActiveEmergenciesCount = ActiveEmergencies.Count;

            MissedAppointmentsWeek = await _context.Appointments
                .CountAsync(a => a.ClinicId == cid.Value && a.Status == "missed" && a.ScheduledDateTime >= weekStart && a.ScheduledDateTime < weekEnd);
        }
        else
        {
            ClinicPatients = 0;
            TodayAppointments = new List<Appointment>();
            ActiveEmergencies = new List<EmergencyCall>();
            TodayAppointmentsCount = 0;
            ActiveEmergenciesCount = 0;
        }

        return Page();
    }
}
