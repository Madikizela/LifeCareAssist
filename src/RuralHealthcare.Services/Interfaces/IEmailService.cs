namespace RuralHealthcare.Services.Interfaces;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string firstName, string temporaryPassword);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendEmergencyAlertEmailAsync(string email, string patientName, string emergencyType);
    Task SendEmailAsync(string email, string subject, string body);
}
