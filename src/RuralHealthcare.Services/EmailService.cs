using Microsoft.Extensions.Configuration;
using RuralHealthcare.Services.Interfaces;

namespace RuralHealthcare.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName, string temporaryPassword)
    {
        var subject = "Welcome to Rural Healthcare Platform";
        var baseUrl = _configuration["App:PublicBaseUrl"] ?? "http://localhost:5000";
        var loginUrl = $"{baseUrl}/login";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Welcome to Rural Healthcare Platform</h2>
                <p>Hello {firstName},</p>
                <p>Your account has been created successfully. Please use the following credentials to login:</p>
                
                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                    <p><strong>Username (Email):</strong> {email}</p>
                    <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                </div>
                
                <p><strong>Important:</strong> You will be required to change your password on first login.</p>
                
                
                <p>Login URL: <a href='{loginUrl}'>{loginUrl}</a></p>
                
                <p>If you have any questions, please contact your system administrator.</p>
                
                <p>Best regards,<br/>Rural Healthcare Platform Team</p>
            </body>
            </html>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var subject = "Password Reset Request";
        var baseUrl = _configuration["App:PublicBaseUrl"] ?? "http://localhost:5000";
        var resetUrl = $"{baseUrl}/account/resetpassword?token={resetToken}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href='{resetUrl}'>Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you did not request this, please ignore this email.</p>
            </body>
            </html>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendEmergencyAlertEmailAsync(string email, string patientName, string emergencyType)
    {
        var subject = $"🚨 EMERGENCY ALERT - {patientName}";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2 style='color: red;'>🚨 EMERGENCY ALERT</h2>
                <p><strong>Patient:</strong> {patientName}</p>
                <p><strong>Emergency Type:</strong> {emergencyType}</p>
                <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                <p>Emergency services have been notified.</p>
            </body>
            </html>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var provider = _configuration["EmailProvider:Provider"] ?? "Console";

        var fromEmail = _configuration["EmailProvider:FromEmail"] ?? "noreply@example.com";
        var fromName = _configuration["EmailProvider:FromName"] ?? "Rural Healthcare Platform";

        if (provider.Equals("SMTP", StringComparison.OrdinalIgnoreCase))
        {
            var host = _configuration["EmailProvider:SmtpHost"] ?? "localhost";
            var portStr = _configuration["EmailProvider:SmtpPort"] ?? "25";
            var username = _configuration["EmailProvider:SmtpUsername"];
            var password = _configuration["EmailProvider:SmtpPassword"];
            var useSslStr = _configuration["EmailProvider:Ssl"] ?? "false";
            var secure = _configuration["EmailProvider:Secure"] ?? "SMTPS"; // SMTPS or STARTTLS
            var skipCertStr = _configuration["EmailProvider:SkipCertificateValidation"] ?? "false";

            int.TryParse(portStr, out var port);
            bool.TryParse(useSslStr, out var useSsl);
            bool.TryParse(skipCertStr, out var skipCert);

            using var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress(fromName, fromEmail));
            message.To.Add(new MimeKit.MailboxAddress(to, to));
            message.Subject = subject;

            var bodyBuilder = new MimeKit.BodyBuilder
            {
                HtmlBody = body,
                TextBody = ""
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            if (skipCert)
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            }
            client.Timeout = 15000;
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            try
            {
                if (secure.Equals("SMTPS", StringComparison.OrdinalIgnoreCase))
                {
                    await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.SslOnConnect);
                }
                else if (secure.Equals("STARTTLS", StringComparison.OrdinalIgnoreCase))
                {
                    await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                }
                else
                {
                    await client.ConnectAsync(host, port, useSsl);
                }
                if (!string.IsNullOrEmpty(username))
                {
                    await client.AuthenticateAsync(username, password);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP send failed: {ex.Message}");
                // Fallback to console logging below
            }
        }

        // Default: console log for development
        Console.WriteLine($"=== EMAIL ===");
        Console.WriteLine($"Provider: {provider}");
        Console.WriteLine($"From: {fromName} <{fromEmail}>");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");
        Console.WriteLine($"=============");
        await Task.CompletedTask;
    }
}
