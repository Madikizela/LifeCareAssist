using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Admin.Users;

public class UsersIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public UsersIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<User> Users { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Check authentication
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole");
        
        if (string.IsNullOrEmpty(userId) || (userRole != "system_admin" && userRole != "clinic_admin"))
        {
            return RedirectToPage("/AccessDenied");
        }

        // Load users based on role
        if (userRole == "clinic_admin")
        {
            var user = await _context.Users.FindAsync(Guid.Parse(userId));
            if (user?.ClinicId.HasValue == true)
            {
                // Clinic admin sees only users from their clinic
                Users = await _context.Users
                    .Where(u => u.ClinicId == user.ClinicId.Value)
                    .OrderBy(u => u.LastName)
                    .ToListAsync();
            }
        }
        else
        {
            // System admin sees all users
            Users = await _context.Users
                .OrderBy(u => u.LastName)
                .ToListAsync();
        }

        return Page();
    }
}
