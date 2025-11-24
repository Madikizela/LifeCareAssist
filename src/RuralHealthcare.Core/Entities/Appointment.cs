namespace RuralHealthcare.Core.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid? ClinicId { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public string AppointmentType { get; set; } = string.Empty; // checkup, chronic, emergency, etc.
    public string Status { get; set; } = "scheduled"; // scheduled, completed, missed, cancelled
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Reminder tracking
    public bool Reminder3DaysSent { get; set; } = false;
    public bool Reminder1DaySent { get; set; } = false;
    public bool ReminderSameDaySent { get; set; } = false;
    
    public Patient Patient { get; set; } = null!;
    public Clinic? Clinic { get; set; }
}
