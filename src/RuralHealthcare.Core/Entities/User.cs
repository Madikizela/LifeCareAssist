namespace RuralHealthcare.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "patient"; // patient, caregiver, health_worker, clinic_admin, system_admin
    public string? PhoneNumber { get; set; }
    public Guid? ClinicId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequirePasswordChange { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
