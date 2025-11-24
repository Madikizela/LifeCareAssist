using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Patients;

public class PatientsIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public PatientsIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Patient> Patients { get; set; } = new();
    public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)]
    public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string? search)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Index");
        }

        SearchTerm = search;
        var query = _context.Patients.AsQueryable();

        // If clinic admin, force filter to their clinic
        if (userRole == "clinic_admin")
        {
            var currentUser = await _context.Users.FindAsync(Guid.Parse(userId));
            if (currentUser?.ClinicId != null)
            {
                var cid = currentUser.ClinicId.Value;
                query = query.Where(p => p.ClinicId == cid);
            }
            else
            {
                // No clinic assigned: show nothing
                query = query.Where(p => false);
            }
        }
        else if (ClinicId.HasValue)
        {
            // Optional filter when navigated from a clinic page
            query = query.Where(p => p.ClinicId == ClinicId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => 
                p.FirstName.Contains(search) ||
                p.LastName.Contains(search) ||
                p.IdNumber.Contains(search) ||
                p.PhoneNumber.Contains(search));
        }

        TotalCount = await query.CountAsync();
        var skip = (Math.Max(1, Page) - 1) * Math.Max(1, PageSize);
        Patients = await query
            .OrderBy(p => p.LastName)
            .Skip(skip)
            .Take(PageSize)
            .AsNoTracking()
            .ToListAsync();

        return Page();
    }
}
