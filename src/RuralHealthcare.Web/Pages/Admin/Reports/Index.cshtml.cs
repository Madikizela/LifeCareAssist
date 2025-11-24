using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Admin.Reports;

public class AdminReportsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AdminReportsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ClinicId { get; set; }

    public List<Clinic> Clinics { get; set; } = new();

    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int MissedAppointments { get; set; }
    public int CancelledAppointments { get; set; }

    public int EmergencyCalls { get; set; }
    public int EmergenciesDispatched { get; set; }
    public int EmergenciesCompleted { get; set; }

    public int NewPatients { get; set; }
    public double AdherenceRate { get; set; }

    public List<(string Date, int Count)> AppointmentsPerDay { get; set; } = new();
    public List<(string ClinicName, int Count)> AppointmentsByClinic { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "system_admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        var start = (StartDate ?? DateTime.UtcNow.AddDays(-30)).Date;
        var end = (EndDate ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);

        Clinics = await _context.Clinics
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        var appointmentsQuery = _context.Appointments
            .Include(a => a.Clinic)
            .Where(a => a.ScheduledDateTime >= start && a.ScheduledDateTime <= end)
            .AsNoTracking()
            .AsQueryable();

        if (ClinicId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.ClinicId == ClinicId.Value);
        }

        TotalAppointments = await appointmentsQuery.CountAsync();
        CompletedAppointments = await appointmentsQuery.CountAsync(a => a.Status == "completed");
        MissedAppointments = await appointmentsQuery.CountAsync(a => a.Status == "missed");
        CancelledAppointments = await appointmentsQuery.CountAsync(a => a.Status == "cancelled");

        var emergencyQuery = _context.EmergencyCalls
            .Where(e => e.CallTime >= start && e.CallTime <= end)
            .AsNoTracking();

        EmergencyCalls = await emergencyQuery.CountAsync();
        EmergenciesDispatched = await emergencyQuery.CountAsync(e => e.Status == "dispatched");
        EmergenciesCompleted = await emergencyQuery.CountAsync(e => e.Status == "completed");

        NewPatients = await _context.Patients
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .CountAsync();

        var logs = await _context.MedicationLogs
            .Where(l => l.ScheduledTime >= start && l.ScheduledTime <= end)
            .Select(l => new { l.WasTaken })
            .ToListAsync();
        var totalLogs = logs.Count;
        var takenLogs = logs.Count(l => l.WasTaken);
        AdherenceRate = totalLogs > 0 ? Math.Round((double)takenLogs / totalLogs * 100, 1) : 0;

        var perDay = await appointmentsQuery
            .GroupBy(a => a.ScheduledDateTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();
        AppointmentsPerDay = perDay
            .Select(x => (x.Date.ToString("yyyy-MM-dd"), x.Count))
            .ToList();

        var byClinic = await appointmentsQuery
            .Where(a => a.Clinic != null)
            .GroupBy(a => a.Clinic!.Name)
            .Select(g => new { ClinicName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        AppointmentsByClinic = byClinic.Select(x => (x.ClinicName, x.Count)).ToList();

        return Page();
    }

    public async Task<IActionResult> OnGetExportCsv()
    {
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "system_admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        var start = (StartDate ?? DateTime.UtcNow.AddDays(-30)).Date;
        var end = (EndDate ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);

        var appointmentsQuery = _context.Appointments
            .Include(a => a.Clinic)
            .Where(a => a.ScheduledDateTime >= start && a.ScheduledDateTime <= end)
            .AsNoTracking();
        if (ClinicId.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.ClinicId == ClinicId.Value);
        }

        var totalAppointments = await appointmentsQuery.CountAsync();
        var completedAppointments = await appointmentsQuery.CountAsync(a => a.Status == "completed");
        var missedAppointments = await appointmentsQuery.CountAsync(a => a.Status == "missed");
        var cancelledAppointments = await appointmentsQuery.CountAsync(a => a.Status == "cancelled");

        var emergencyQuery = _context.EmergencyCalls
            .Where(e => e.CallTime >= start && e.CallTime <= end)
            .AsNoTracking();
        var emergencyCalls = await emergencyQuery.CountAsync();
        var emergenciesDispatched = await emergencyQuery.CountAsync(e => e.Status == "dispatched");
        var emergenciesCompleted = await emergencyQuery.CountAsync(e => e.Status == "completed");

        var newPatients = await _context.Patients
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end)
            .CountAsync();

        var logs = await _context.MedicationLogs
            .Where(l => l.ScheduledTime >= start && l.ScheduledTime <= end)
            .Select(l => new { l.WasTaken })
            .ToListAsync();
        var totalLogs = logs.Count;
        var takenLogs = logs.Count(l => l.WasTaken);
        var adherenceRate = totalLogs > 0 ? Math.Round((double)takenLogs / totalLogs * 100, 1) : 0;

        var perDay = await appointmentsQuery
            .GroupBy(a => a.ScheduledDateTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var byClinic = await appointmentsQuery
            .Where(a => a.Clinic != null)
            .GroupBy(a => a.Clinic!.Name)
            .Select(g => new { ClinicName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"TotalAppointments,{totalAppointments}");
        sb.AppendLine($"CompletedAppointments,{completedAppointments}");
        sb.AppendLine($"MissedAppointments,{missedAppointments}");
        sb.AppendLine($"CancelledAppointments,{cancelledAppointments}");
        sb.AppendLine($"EmergencyCalls,{emergencyCalls}");
        sb.AppendLine($"EmergenciesDispatched,{emergenciesDispatched}");
        sb.AppendLine($"EmergenciesCompleted,{emergenciesCompleted}");
        sb.AppendLine($"NewPatients,{newPatients}");
        sb.AppendLine($"AdherenceRatePercent,{adherenceRate}");
        sb.AppendLine();
        sb.AppendLine("AppointmentsPerDay,Count");
        foreach (var d in perDay)
        {
            sb.AppendLine($"{d.Date:yyyy-MM-dd},{d.Count}");
        }
        sb.AppendLine();
        sb.AppendLine("AppointmentsByClinic,Count");
        foreach (var c in byClinic)
        {
            sb.AppendLine($"{c.ClinicName},{c.Count}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"reports_{start:yyyyMMdd}_{end:yyyyMMdd}{(ClinicId.HasValue ? "_clinic" : "")}.csv";
        return File(bytes, "text/csv", fileName);
    }
}

