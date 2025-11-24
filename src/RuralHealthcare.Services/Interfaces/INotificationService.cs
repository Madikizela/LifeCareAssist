namespace RuralHealthcare.Services.Interfaces;

public interface INotificationService
{
    Task SendSmsAsync(string phoneNumber, string message, string language = "en");
    Task SendVoiceCallAsync(string phoneNumber, string message, string language = "en");
    Task SendMedicationReminderAsync(Guid patientId, Guid medicationId);
    Task SendAppointmentReminderAsync(Guid patientId, Guid appointmentId);
    Task SendEmergencyAlertAsync(Guid emergencyCallId);
}
