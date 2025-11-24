namespace RuralHealthcare.Core.Entities;

public class EmergencyCall
{
    public Guid Id { get; set; }
    public Guid? PatientId { get; set; } // Nullable for anonymous calls
    
    // Anonymous caller information (when PatientId is null)
    public string? CallerName { get; set; }
    public string? CallerPhone { get; set; }
    public string? CallerIdNumber { get; set; }
    
    public string EmergencyType { get; set; } = "medical"; // medical, security
    public DateTime CallTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationDescription { get; set; } // For when GPS is not available
    public string Status { get; set; } = "pending"; // pending, dispatched, arrived, completed, cancelled
    public string? Description { get; set; }
    public Guid? AssignedAmbulanceId { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public Patient? Patient { get; set; }
}
