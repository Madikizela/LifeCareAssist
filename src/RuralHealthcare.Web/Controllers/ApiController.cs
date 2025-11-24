using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Infrastructure.Data;

namespace RuralHealthcare.Web.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("patients/{id}/medications")]
    public async Task<IActionResult> GetPatientMedications(Guid id)
    {
        var medications = await _context.Medications
            .Where(m => m.PatientId == id && m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Dosage,
                m.Frequency,
                m.ReminderTimes,
                m.Instructions
            })
            .ToListAsync();

        return Ok(medications);
    }

    [HttpPost("medications/{id}/log")]
    public async Task<IActionResult> LogMedicationTaken(Guid id, [FromBody] LogMedicationRequest request)
    {
        var medication = await _context.Medications.FindAsync(id);
        if (medication == null)
            return NotFound();

        var log = new Core.Entities.MedicationLog
        {
            Id = Guid.NewGuid(),
            MedicationId = id,
            ScheduledTime = request.ScheduledTime,
            TakenTime = DateTime.UtcNow,
            WasTaken = true,
            Notes = request.Notes
        };

        _context.MedicationLogs.Add(log);
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    [HttpGet("emergency/active")]
    public async Task<IActionResult> GetActiveEmergencies()
    {
        var emergencies = await _context.EmergencyCalls
            .Where(e => e.Status == "pending" || e.Status == "dispatched")
            .Include(e => e.Patient)
            .OrderByDescending(e => e.CallTime)
            .Select(e => new
            {
                e.Id,
                e.EmergencyType,
                e.CallTime,
                e.Status,
                e.Latitude,
                e.Longitude,
                Patient = new
                {
                    e.Patient.FirstName,
                    e.Patient.LastName,
                    e.Patient.PhoneNumber
                }
            })
            .ToListAsync();

        return Ok(emergencies);
    }
}

public class LogMedicationRequest
{
    public DateTime ScheduledTime { get; set; }
    public string? Notes { get; set; }
}
