namespace RuralHealthcare.Core.Entities;

public class Patient
{
    public Guid Id { get; set; }
    public string IdNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternativePhoneNumber { get; set; }
    public string PreferredLanguage { get; set; } = "en"; // en, zu, xh, st, tn
    public string? HomeAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Guid? ClinicId { get; set; }
    
    // Medical Information
    public List<string> ChronicConditions { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public string? BloodType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Clinic? Clinic { get; set; }
    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<EmergencyCall> EmergencyCalls { get; set; } = new List<EmergencyCall>();
}
