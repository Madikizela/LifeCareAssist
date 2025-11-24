using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Pages.Clinics;

public class FindMedicationModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public FindMedicationModel(ApplicationDbContext context) { _context = context; }

    [BindProperty(SupportsGet = true)] public string? Query { get; set; }
    [BindProperty(SupportsGet = true)] public double? Latitude { get; set; }
    [BindProperty(SupportsGet = true)] public double? Longitude { get; set; }
    [BindProperty(SupportsGet = true)] public bool InStockOnly { get; set; } = true;
    [BindProperty(SupportsGet = true)] public string? Category { get; set; }
    [BindProperty(SupportsGet = true)] public int? RadiusKm { get; set; }

    public List<ResultItem> Results { get; set; } = new();

    public class ResultItem
    {
        public Core.Entities.Clinic Clinic { get; set; } = null!;
        public List<string> MatchedMedications { get; set; } = new();
        public double? DistanceKm { get; set; }
        public string? DistanceLabel { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var clinics = await _context.Clinics
            .Where(c => c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(Query) || !string.IsNullOrWhiteSpace(Category))
        {
            var q = Query?.Trim().ToLowerInvariant();
            var cat = Category?.Trim();
            clinics = clinics
                .Where(c => c.MedicationStock != null && c.MedicationStock.Any(s =>
                    (string.IsNullOrWhiteSpace(q) || s.Name.ToLower().Contains(q)) &&
                    (string.IsNullOrWhiteSpace(cat) || string.Equals(s.Category, cat, StringComparison.OrdinalIgnoreCase)) &&
                    (!InStockOnly || s.InStock)))
                .ToList();
        }

        var res = new List<ResultItem>();
        foreach (var c in clinics)
        {
            var matched = new List<string>();
            if (c.MedicationStock != null)
            {
                var q = Query?.Trim().ToLowerInvariant();
                var cat = Category?.Trim();
                matched = c.MedicationStock
                    .Where(s =>
                        (string.IsNullOrWhiteSpace(q) || s.Name.ToLower().Contains(q)) &&
                        (string.IsNullOrWhiteSpace(cat) || string.Equals(s.Category, cat, StringComparison.OrdinalIgnoreCase)) &&
                        (!InStockOnly || s.InStock))
                    .Select(s => $"{s.Name} (Qty: {s.Quantity})")
                    .Take(5)
                    .ToList();
            }
            var item = new ResultItem { Clinic = c, MatchedMedications = matched };
            if (Latitude.HasValue && Longitude.HasValue)
            {
                var d = Haversine(Latitude.Value, Longitude.Value, c.Latitude, c.Longitude);
                item.DistanceKm = d;
                item.DistanceLabel = $"{Math.Round(d, 1)} km away";
            }
            res.Add(item);
        }

        if (Latitude.HasValue && Longitude.HasValue)
        {
            if (RadiusKm.HasValue)
            {
                res = res.Where(r => r.DistanceKm.HasValue && r.DistanceKm.Value <= RadiusKm.Value).ToList();
            }
            Results = res.OrderBy(r => r.DistanceKm ?? double.MaxValue).ToList();
        }
        else
        {
            Results = res;
        }

        return Page();
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
