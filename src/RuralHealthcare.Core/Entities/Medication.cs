namespace RuralHealthcare.Core.Entities;

public class Medication
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty; // daily, twice_daily, weekly, etc.
    public List<TimeOnly> ReminderTimes { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Patient Patient { get; set; } = null!;
    public ICollection<MedicationLog> Logs { get; set; } = new List<MedicationLog>();
}

public class MedicationLog
{
    public Guid Id { get; set; }
    public Guid MedicationId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? TakenTime { get; set; }
    public bool WasTaken { get; set; }
    public string? Notes { get; set; }
    public Guid? RecordedByUserId { get; set; }
    
    public Medication Medication { get; set; } = null!;
}
