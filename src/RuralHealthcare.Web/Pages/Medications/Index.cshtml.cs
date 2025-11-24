using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Medications;

public class MedicationsIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public MedicationsIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Medication> Medications { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? Filter { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search, string? filter)
    {
        // Check authentication
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        SearchTerm = search;
        Filter = filter;

        var query = _context.Medications
            .Include(m => m.Patient)
            .AsQueryable();

        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId != null)
            {
                var cid = currentUser.ClinicId.Value;
                query = query.Where(m => m.Patient.ClinicId == cid);
            }
            else
            {
                query = query.Where(m => false);
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m =>
                m.Name.Contains(search) ||
                m.Patient.FirstName.Contains(search) ||
                m.Patient.LastName.Contains(search));
        }

        if (filter == "active")
        {
            query = query.Where(m => m.IsActive);
        }
        else if (filter == "inactive")
        {
            query = query.Where(m => !m.IsActive);
        }

        Medications = await query
            .OrderBy(m => m.Patient.LastName)
            .ThenBy(m => m.Name)
            .ToListAsync();

        return Page();
    }
}
