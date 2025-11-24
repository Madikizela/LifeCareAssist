namespace RuralHealthcare.Core.Entities;

public class Clinic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? OperatingHours { get; set; }
    public bool HasAmbulance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    public List<string> AvailableMedications { get; set; } = new();
    public List<ClinicMedicationItem> MedicationStock { get; set; } = new();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}

public class ClinicMedicationItem
{
    public string Name { get; set; } = string.Empty;
    public bool InStock { get; set; } = true;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; } = 0;
    public int LowThreshold { get; set; } = 0;
}
