using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Admin.Clinics;

public class ClinicsIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ClinicsIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Clinic> Clinics { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? Filter { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, string? filter)
    {
        // Check authentication and authorization
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || (userRole != "system_admin" && userRole != "clinic_admin"))
        {
            return RedirectToPage("/AccessDenied");
        }

        SearchTerm = search;
        Filter = filter;

        var query = _context.Clinics.AsQueryable();

        // Clinic admin can only see their assigned clinic
        if (userRole == "clinic_admin")
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user?.ClinicId.HasValue == true)
            {
                query = query.Where(c => c.Id == user.ClinicId.Value);
            }
            else
            {
                // Clinic admin without assigned clinic - show nothing
                Clinics = new List<Clinic>();
                return Page();
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => 
                c.Name.Contains(search) ||
                c.Address.Contains(search) ||
                c.PhoneNumber.Contains(search));
        }

        if (filter == "active")
        {
            query = query.Where(c => c.IsActive);
        }
        else if (filter == "with_ambulance")
        {
            query = query.Where(c => c.HasAmbulance);
        }
        else if (filter == "stock_issues")
        {
            // Filter clinics that have any out-of-stock or low-stock items
            var all = await query.ToListAsync();
            all = all.Where(c =>
            {
                var stock = c.MedicationStock ?? new List<ClinicMedicationItem>();
                return stock.Any(i => !i.InStock) || stock.Any(i => i.InStock && i.Quantity <= i.LowThreshold);
            }).ToList();
            Clinics = all.OrderBy(c => c.Name).ToList();
            return Page();
        }

        Clinics = await query.OrderBy(c => c.Name).ToListAsync();
        return Page();
    }
}
