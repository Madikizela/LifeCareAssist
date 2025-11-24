using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;
using RuralHealthcare.Core.Entities;

namespace RuralHealthcare.Web.Pages.Admin;

public class AdminDashboardModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalPatients { get; set; }
    public int ActiveClinics { get; set; }
    public int HealthWorkers { get; set; }
    public int EmergencyCalls24h { get; set; }
    public List<string> ChartLabels7 { get; set; } = new();
    public List<int> Appointments7 { get; set; } = new();
    public List<int> Emergencies7 { get; set; } = new();
    public List<int> Adherence7 { get; set; } = new();
    public int OverallAdherencePercent { get; set; }
    public int AppointmentsToday { get; set; }
    public int MissedDoseAlertPatients { get; set; }
    public List<ActivityItem> RecentActivities { get; set; } = new();
    public int OutOfStockItems { get; set; }
    public int LowStockItems { get; set; }

    public class ActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public DateTime OccurredAt { get; set; }
        public string When { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check authentication
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        // Check authorization - only admins
        var userRole = HttpContext.Session.GetString("UserRole");
        if (userRole != "system_admin" && userRole != "clinic_admin")
        {
            return RedirectToPage("/AccessDenied");
        }

        // Get user's clinic if clinic admin
        var user = await _context.Users.FindAsync(Guid.Parse(userId));
        var userClinicId = user?.ClinicId;

        // Scope data based on role
        if (userRole == "clinic_admin" && userClinicId.HasValue)
        {
            var cid = userClinicId.Value;
            TotalPatients = await _context.Patients.CountAsync(p => p.ClinicId == cid);
            ActiveClinics = 1;
            HealthWorkers = await _context.Users.CountAsync(u => u.Role == "health_worker" && u.ClinicId == cid);

            var yesterday = DateTime.UtcNow.AddHours(-24);
            EmergencyCalls24h = await _context.EmergencyCalls
                .Include(e => e.Patient)
                .CountAsync(e => e.CallTime >= yesterday && e.Patient != null && e.Patient.ClinicId == cid);

            await PopulateChartsAsync(cid);
            await PopulateRecentActivityAsync(cid);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            AppointmentsToday = await _context.Appointments
                .CountAsync(a => a.ClinicId == cid && a.ScheduledDateTime >= today && a.ScheduledDateTime < tomorrow);

            var clinic = await _context.Clinics.FindAsync(cid);
            var stock = clinic?.MedicationStock ?? new List<ClinicMedicationItem>();
            OutOfStockItems = stock.Count(i => !i.InStock);
            LowStockItems = stock.Count(i => i.InStock && i.Quantity <= i.LowThreshold);
        }
        else
        {
            TotalPatients = await _context.Patients.CountAsync();
            ActiveClinics = await _context.Clinics.CountAsync(c => c.IsActive);
            HealthWorkers = await _context.Users.CountAsync(u => u.Role == "health_worker");

            var yesterday = DateTime.UtcNow.AddHours(-24);
            EmergencyCalls24h = await _context.EmergencyCalls
                .CountAsync(e => e.CallTime >= yesterday);

            await PopulateChartsAsync(null);
            await PopulateRecentActivityAsync(null);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            AppointmentsToday = await _context.Appointments
                .CountAsync(a => a.ScheduledDateTime >= today && a.ScheduledDateTime < tomorrow);

            var clinics = await _context.Clinics.ToListAsync();
            OutOfStockItems = clinics.Sum(c => (c.MedicationStock ?? new List<ClinicMedicationItem>()).Count(i => !i.InStock));
            LowStockItems = clinics.Sum(c => (c.MedicationStock ?? new List<ClinicMedicationItem>()).Count(i => i.InStock && i.Quantity <= i.LowThreshold));
        }

        return Page();
    }

    private async Task PopulateChartsAsync(Guid? clinicId)
    {
        var start = DateTime.Today.AddDays(-6);
        var endExclusive = start.AddDays(7);
        var days = Enumerable.Range(0, 7).Select(i => start.AddDays(i)).ToList();
        ChartLabels7 = days.Select(d => d.ToString("dd MMM")).ToList();

        var appointmentsQuery = _context.Appointments
            .Where(a => a.ScheduledDateTime >= start && a.ScheduledDateTime < endExclusive);
        if (clinicId.HasValue)
        {
            var cid = clinicId.Value;
            appointmentsQuery = appointmentsQuery.Where(a => a.ClinicId == cid);
        }
        var apptGroups = await appointmentsQuery
            .GroupBy(a => a.ScheduledDateTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        Appointments7 = days.Select(d => apptGroups.FirstOrDefault(x => x.Date == d.Date)?.Count ?? 0).ToList();

        var emergenciesQuery = _context.EmergencyCalls
            .Include(e => e.Patient)
            .Where(e => e.CallTime >= start && e.CallTime < endExclusive);
        if (clinicId.HasValue)
        {
            var cid = clinicId.Value;
            emergenciesQuery = emergenciesQuery.Where(e => e.Patient != null && e.Patient.ClinicId == cid);
        }
        var emerGroups = await emergenciesQuery
            .GroupBy(e => e.CallTime.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();
        Emergencies7 = days.Select(d => emerGroups.FirstOrDefault(x => x.Date == d.Date)?.Count ?? 0).ToList();

        var logsQuery = _context.MedicationLogs
            .Include(l => l.Medication)
            .ThenInclude(m => m.Patient)
            .Where(l => l.ScheduledTime >= start && l.ScheduledTime < endExclusive);
        if (clinicId.HasValue)
        {
            var cid = clinicId.Value;
            logsQuery = logsQuery.Where(l => l.Medication.Patient.ClinicId == cid);
        }
        logsQuery = logsQuery.Where(l => l.Medication.Patient != null);
        var logs = await logsQuery
            .Select(l => new { Day = l.ScheduledTime.Date, l.WasTaken, PatientId = l.Medication.Patient.Id })
            .ToListAsync();
        Adherence7 = days.Select(d =>
        {
            var dayLogs = logs.Where(x => x.Day == d.Date).ToList();
            var total = dayLogs.Count;
            var taken = dayLogs.Count(x => x.WasTaken);
            var pct = total > 0 ? (int)Math.Round((double)taken * 100 / total) : 0;
            return pct;
        }).ToList();

        var totalLogs = logs.Count;
        var totalTaken = logs.Count(x => x.WasTaken);
        OverallAdherencePercent = totalLogs > 0 ? (int)Math.Round((double)totalTaken * 100 / totalLogs) : 0;

        var alertStart = DateTime.Today.AddDays(-29);
        var alertEndExclusive = alertStart.AddDays(30);
        var alertLogsQuery = _context.MedicationLogs
            .Include(l => l.Medication)
            .ThenInclude(m => m.Patient)
            .Where(l => l.ScheduledTime >= alertStart && l.ScheduledTime < alertEndExclusive);
        if (clinicId.HasValue)
        {
            var cid = clinicId.Value;
            alertLogsQuery = alertLogsQuery.Where(l => l.Medication.Patient.ClinicId == cid);
        }
        var alertLogs = await alertLogsQuery
            .Select(l => new { PatientId = l.Medication.Patient.Id, l.WasTaken })
            .ToListAsync();
        var missedGroups = alertLogs.Where(x => !x.WasTaken)
            .GroupBy(x => x.PatientId)
            .Select(g => new { PatientId = g.Key, Count = g.Count() })
            .ToList();
        MissedDoseAlertPatients = missedGroups.Count(x => x.Count >= 3);
    }

    private async Task PopulateRecentActivityAsync(Guid? clinicId)
    {
        var items = new List<ActivityItem>();

        var patientsQuery = _context.Patients.AsQueryable();
        if (clinicId.HasValue)
        {
            var cid = clinicId.Value;
            patientsQuery = patientsQuery.Where(p => p.ClinicId == cid);
        }
        var recentPatients = await patientsQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new ActivityItem
            {
                Title = "New patient registered",
                Detail = p.FirstName + " " + p.LastName,
                OccurredAt = p.CreatedAt,
                When = FormatTimeAgo(p.CreatedAt)
            })
            .ToListAsync();
        items.AddRange(recentPatients);

        var emergQuery = _context.EmergencyCalls
            .Include(e => e.Patient)
            .AsQueryable();
        if (clinicId.HasValue)
        {
            var cid = clinicId.Value;
            emergQuery = emergQuery.Where(e => e.Patient != null && e.Patient.ClinicId == cid);
        }
        var recentDispatched = await emergQuery
            .Where(e => e.DispatchedAt != null)
            .OrderByDescending(e => e.DispatchedAt)
            .Take(5)
            .Select(e => new ActivityItem
            {
                Title = "Emergency call dispatched",
                Detail = e.Patient != null ? e.Patient.FirstName + " " + e.Patient.LastName : null,
                OccurredAt = e.DispatchedAt!.Value,
                When = FormatTimeAgo(e.DispatchedAt!.Value)
            })
            .ToListAsync();
        items.AddRange(recentDispatched);

        var clinicsQuery = _context.Clinics.AsQueryable();
        var recentClinics = await clinicsQuery
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new ActivityItem
            {
                Title = "Clinic added",
                Detail = c.Name,
                OccurredAt = c.CreatedAt,
                When = FormatTimeAgo(c.CreatedAt)
            })
            .ToListAsync();
        items.AddRange(recentClinics);

        RecentActivities = items
            .OrderByDescending(i => i.OccurredAt)
            .Take(10)
            .ToList();
    }

    private static string FormatTimeAgo(DateTime dt)
    {
        var now = DateTime.UtcNow;
        var ts = now - (dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime());
        if (ts.TotalSeconds < 60) return "just now";
        if (ts.TotalMinutes < 60) return $"{Math.Floor(ts.TotalMinutes)} minutes ago";
        if (ts.TotalHours < 24) return $"{Math.Floor(ts.TotalHours)} hours ago";
        return $"{Math.Floor(ts.TotalDays)} days ago";
    }
}
