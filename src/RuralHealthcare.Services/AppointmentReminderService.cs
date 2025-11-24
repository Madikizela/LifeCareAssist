using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RuralHealthcare.Infrastructure.Data;
using RuralHealthcare.Services.Interfaces;

namespace RuralHealthcare.Services;

public class AppointmentReminderService : BackgroundService
{
    private readonly ILogger<AppointmentReminderService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AppointmentReminderService(
        ILogger<AppointmentReminderService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Appointment Reminder Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendAppointmentReminders();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending appointment reminders");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SendAppointmentReminders()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var now = DateTime.UtcNow;
        var today = now.Date;

        // Get appointments that need reminders
        var appointments = await context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Clinic)
            .Where(a => a.Status == "scheduled")
            .Where(a => a.ScheduledDateTime.Date >= today)
            .ToListAsync();

        foreach (var appointment in appointments)
        {
            var daysUntil = (appointment.ScheduledDateTime.Date - today).Days;
            
            // Check if we should send a reminder
            bool shouldSend = daysUntil switch
            {
                3 => !appointment.Reminder3DaysSent,
                1 => !appointment.Reminder1DaySent,
                0 => !appointment.ReminderSameDaySent,
                _ => false
            };

            if (shouldSend && !string.IsNullOrEmpty(appointment.Patient.PhoneNumber))
            {
                try
                {
                    var subject = daysUntil switch
                    {
                        3 => "Appointment Reminder - 3 Days",
                        1 => "Appointment Reminder - Tomorrow",
                        0 => "Appointment Reminder - Today",
                        _ => "Appointment Reminder"
                    };

                    var message = $@"
Dear {appointment.Patient.FirstName} {appointment.Patient.LastName},

This is a reminder about your upcoming appointment:

Date: {appointment.ScheduledDateTime:dddd, MMMM dd, yyyy}
Time: {appointment.ScheduledDateTime:HH:mm}
Type: {appointment.AppointmentType.Replace("_", " ")}
Clinic: {appointment.Clinic?.Name ?? "Not specified"}

{(daysUntil == 0 ? "Your appointment is TODAY!" : $"Your appointment is in {daysUntil} day(s).")}

{(!string.IsNullOrEmpty(appointment.Notes) ? $"Notes: {appointment.Notes}" : "")}

Please arrive 10 minutes early.

If you need to reschedule, please contact us as soon as possible.

Best regards,
Rural Healthcare Platform
";

                    // Send email (free)
                    await emailService.SendEmailAsync(
                        appointment.Patient.PhoneNumber + "@example.com", // Using phone as email for now
                        subject,
                        message
                    );

                    // Mark as sent
                    switch (daysUntil)
                    {
                        case 3:
                            appointment.Reminder3DaysSent = true;
                            break;
                        case 1:
                            appointment.Reminder1DaySent = true;
                            break;
                        case 0:
                            appointment.ReminderSameDaySent = true;
                            break;
                    }

                    await context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Sent {Days}-day reminder for appointment {AppointmentId} to patient {PatientName}",
                        daysUntil,
                        appointment.Id,
                        $"{appointment.Patient.FirstName} {appointment.Patient.LastName}"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Failed to send reminder for appointment {AppointmentId}", 
                        appointment.Id
                    );
                }
            }
        }
    }
}
