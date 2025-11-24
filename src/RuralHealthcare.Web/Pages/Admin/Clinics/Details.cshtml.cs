using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;
using RuralHealthcare.Services.Interfaces;

namespace RuralHealthcare.Web.Pages.Admin.Clinics;

public class ClinicDetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ClinicDetailsModel(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public Clinic Clinic { get; set; } = null!;
    public int TotalPatients { get; set; }
    public int AppointmentsToday { get; set; }

    [BindProperty]
    public string? NewMedicationName { get; set; }
    [BindProperty]
    public string? NewMedicationCategory { get; set; }
    [BindProperty]
    public bool NewMedicationInStock { get; set; }
    [BindProperty]
    public int NewMedicationQuantity { get; set; }
    [BindProperty]
    public int NewMedicationLowThreshold { get; set; }
    [BindProperty]
    public string? ToggleName { get; set; }
    [BindProperty]
    public bool ToggleToStock { get; set; }
    [BindProperty]
    public string? DispenseName { get; set; }
    [BindProperty]
    public int DispenseQuantity { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || (userRole != "system_admin" && userRole != "clinic_admin"))
        {
            return RedirectToPage("/AccessDenied");
        }
        var clinicId = id ?? (Guid.TryParse(Request.Query["id"], out var qid) ? qid : Guid.Empty);
        if (clinicId == Guid.Empty)
        {
            return NotFound();
        }

        var clinic = await _context.Clinics.FindAsync(clinicId);
        if (clinic == null)
        {
            return NotFound();
        }

        Clinic = clinic;

        // Get statistics
        TotalPatients = await _context.Patients.CountAsync();
        
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        AppointmentsToday = await _context.Appointments
            .CountAsync(a => a.ClinicId == clinicId && 
                           a.ScheduledDateTime >= today && 
                           a.ScheduledDateTime < tomorrow);

        return Page();
    }

    public async Task<IActionResult> OnPostAddMedicationAsync(Guid? id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId) || (userRole != "system_admin" && userRole != "clinic_admin"))
        {
            return RedirectToPage("/AccessDenied");
        }

        var clinicId = id ?? (Guid.TryParse(Request.Query["id"], out var qid) ? qid : Guid.Empty);
        if (clinicId == Guid.Empty)
        {
            return NotFound();
        }

        var clinic = await _context.Clinics.FindAsync(clinicId);
        if (clinic == null)
        {
            return NotFound();
        }

        var name = NewMedicationName?.Trim();
        var category = NewMedicationCategory?.Trim();
        var inStock = NewMedicationInStock;
        if (!string.IsNullOrWhiteSpace(name))
        {
            var stock = (clinic.MedicationStock ?? new List<ClinicMedicationItem>()).ToList();
            if (!stock.Any(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                var qty = NewMedicationQuantity;
                var thr = NewMedicationLowThreshold;
                if (qty <= 0) { inStock = false; }
                stock.Add(new ClinicMedicationItem { Name = name, Category = category ?? string.Empty, InStock = inStock, Quantity = Math.Max(0, qty), LowThreshold = Math.Max(0, thr) });
                clinic.MedicationStock = stock;
                _context.Update(clinic);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Added {name} to stock.";
            }
            else
            {
                TempData["ErrorMessage"] = $"{name} already exists in stock.";
            }
        }

        return RedirectToPage("/Admin/Clinics/Details", new { id = clinicId });
    }

    public async Task<IActionResult> OnPostToggleStockAsync(Guid? id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId) || (userRole != "system_admin" && userRole != "clinic_admin"))
        {
            return RedirectToPage("/AccessDenied");
        }

        var clinicId = id ?? (Guid.TryParse(Request.Query["id"], out var qid) ? qid : Guid.Empty);
        if (clinicId == Guid.Empty)
        {
            return NotFound();
        }

        var clinic = await _context.Clinics.FindAsync(clinicId);
        if (clinic == null)
        {
            return NotFound();
        }

        var name = ToggleName?.Trim();
        var desired = ToggleToStock;
        if (!string.IsNullOrWhiteSpace(name))
        {
            var stock = (clinic.MedicationStock ?? new List<ClinicMedicationItem>()).ToList();
            var item = stock.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.InStock = desired;
                clinic.MedicationStock = stock;
                _context.Update(clinic);
                await _context.SaveChangesAsync();
            }
        }

        return RedirectToPage("/Admin/Clinics/Details", new { id = clinicId });
    }

    public async Task<IActionResult> OnPostDispenseMedicationAsync(Guid? id)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId) || (userRole != "system_admin" && userRole != "clinic_admin"))
        {
            return RedirectToPage("/AccessDenied");
        }

        var clinicId = id ?? (Guid.TryParse(Request.Query["id"], out var qid) ? qid : Guid.Empty);
        if (clinicId == Guid.Empty)
        {
            return NotFound();
        }

        var clinic = await _context.Clinics.FindAsync(clinicId);
        if (clinic == null)
        {
            return NotFound();
        }

        var name = DispenseName?.Trim();
        var amount = Math.Max(1, DispenseQuantity);
        if (!string.IsNullOrWhiteSpace(name))
        {
            var stock = (clinic.MedicationStock ?? new List<ClinicMedicationItem>()).ToList();
            var item = stock.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Quantity = Math.Max(0, item.Quantity - amount);
                if (item.Quantity == 0) { item.InStock = false; }
                clinic.MedicationStock = stock;
                _context.Update(clinic);
                await _context.SaveChangesAsync();

                if (item.Quantity <= item.LowThreshold)
                {
                    var admins = await _context.Users.Where(u => u.Role == "system_admin").ToListAsync();
                    foreach (var admin in admins)
                    {
                        var subject = $"Low Stock Alert: {item.Name} at {clinic.Name}";
                        var body = $"Medication '{item.Name}' is low at clinic '{clinic.Name}'. Quantity: {item.Quantity}, Threshold: {item.LowThreshold}.";
                        await _emailService.SendEmailAsync(admin.Email, subject, body);
                    }
                }
            }
        }

        return RedirectToPage("/Admin/Clinics/Details", new { id = clinicId });
    }
}
