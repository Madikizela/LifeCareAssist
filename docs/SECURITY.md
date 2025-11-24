# Security Implementation Guide

## Overview

The Rural Healthcare Platform implements multiple layers of security to protect patient data and ensure POPIA compliance.

## Authentication & Authorization

### Session-Based Authentication
- **Session Timeout**: 30 minutes of inactivity
- **HttpOnly Cookies**: Prevents XSS attacks
- **Secure Cookies**: HTTPS-only in production
- **SameSite Policy**: Strict mode to prevent CSRF

### Role-Based Access Control (RBAC)

#### Roles & Permissions

**System Admin**
- Full system access
- Manage all users and clinics
- View all patients
- System configuration
- Access: All pages

**Clinic Admin**
- Manage assigned clinic
- View clinic patients
- Manage clinic staff
- Clinic-level reports
- Access: Admin dashboard, clinic pages, assigned patients

**Health Worker**
- View assigned patients
- Record home visits
- Update patient information
- Submit referrals
- Access: Patient pages, emergency calls

**Caregiver**
- View assigned patients only
- Receive medication alerts
- View appointment schedules
- Access: Limited patient information

### Protected Routes

#### Public Access (No Login Required)
- `/` - Landing page
- `/emergency/create` - Emergency call
- `/login` - Login page

#### Authenticated Access (Any Logged-in User)
- `/patients` - Patient list
- `/patients/{id}` - Patient details
- `/medications` - Medication management
- `/appointments` - Appointment scheduling

#### Admin Only
- `/admin/dashboard` - System dashboard
- `/admin/users` - User management
- `/admin/clinics` - Clinic management
- `/admin/reports` - System reports

## Security Headers

### Implemented Headers

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Strict-Transport-Security: max-age=31536000; includeSubDomains (Production only)
```

### What They Do

- **X-Content-Type-Options**: Prevents MIME type sniffing
- **X-Frame-Options**: Prevents clickjacking attacks
- **X-XSS-Protection**: Enables browser XSS filter
- **Referrer-Policy**: Controls referrer information
- **HSTS**: Forces HTTPS connections

## Password Security

### Password Requirements
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

### Password Storage
- **Algorithm**: BCrypt with salt
- **Work Factor**: 10 (default)
- **Never stored in plain text**
- **One-way hashing** (cannot be reversed)

### Password Generation
- Cryptographically secure random generation
- 12 characters by default
- Includes all character types
- Unique for each user

## Session Management

### Session Security
```csharp
options.IdleTimeout = TimeSpan.FromMinutes(30);
options.Cookie.HttpOnly = true;
options.Cookie.IsEssential = true;
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
options.Cookie.SameSite = SameSiteMode.Strict;
```

### Session Validation
- Middleware checks authentication on every request
- Automatic redirect to login if session expired
- Audit logging of all access attempts

### Session Data Stored
- User ID (GUID)
- Email address
- Role
- Full name
- Last login time

## Anti-Forgery Protection

### CSRF Tokens
- Automatically generated for forms
- Validated on POST requests
- Header name: `X-CSRF-TOKEN`

### Implementation
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});
```

## Data Protection

### Database Security
- **Encryption at Rest**: SQLite/PostgreSQL encryption
- **Connection Strings**: Stored in secure configuration
- **Parameterized Queries**: Prevents SQL injection
- **Entity Framework**: Built-in SQL injection protection

### Sensitive Data
- Passwords: BCrypt hashed
- Patient data: Access controlled by role
- Audit logs: Track all data access
- PII: Encrypted in transit (HTTPS)

## Audit Logging

### What's Logged
- User login attempts (success/failure)
- Page access by authenticated users
- User creation and modification
- Patient data access
- Emergency calls
- System configuration changes

### Log Format
```
[Timestamp] User {Email} accessed {Path}
[Timestamp] Failed login attempt for {Email}
[Timestamp] User {Email} created by {AdminEmail}
```

### Log Storage
- Application logs: Console (Development)
- Production: File system or cloud logging
- Retention: 7 years (POPIA compliance)

## POPIA Compliance

### Personal Information Protection
- **Consent**: Required for data collection
- **Access Control**: Role-based restrictions
- **Data Minimization**: Only collect necessary data
- **Right to Access**: Patients can view their data
- **Right to Delete**: Account deletion available
- **Breach Notification**: Automated alerts

### Data Subject Rights
1. Right to access personal information
2. Right to correction
3. Right to deletion
4. Right to object to processing
5. Right to data portability

## Vulnerability Protection

### SQL Injection
- **Protection**: Entity Framework parameterized queries
- **Validation**: Input validation on all forms
- **Sanitization**: Automatic by EF Core

### Cross-Site Scripting (XSS)
- **Protection**: Razor automatic encoding
- **Headers**: X-XSS-Protection enabled
- **Content Security Policy**: Implemented

### Cross-Site Request Forgery (CSRF)
- **Protection**: Anti-forgery tokens
- **SameSite Cookies**: Strict mode
- **Validation**: Automatic on POST requests

### Clickjacking
- **Protection**: X-Frame-Options: DENY
- **Prevention**: Cannot be embedded in iframes

### Man-in-the-Middle (MITM)
- **Protection**: HTTPS enforced
- **HSTS**: Strict Transport Security
- **Certificate Validation**: Required

## Production Security Checklist

### Before Deployment

- [ ] Change default admin password
- [ ] Enable HTTPS with valid certificate
- [ ] Configure secure connection strings
- [ ] Enable production logging
- [ ] Set up database backups
- [ ] Configure email provider
- [ ] Enable SMS provider
- [ ] Set up monitoring and alerts
- [ ] Review and test all security headers
- [ ] Perform security audit
- [ ] Test authentication flows
- [ ] Verify role-based access
- [ ] Enable rate limiting
- [ ] Configure firewall rules
- [ ] Set up intrusion detection

### Environment Variables

```bash
# Production
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<secure-connection-string>
EmailProvider__ApiKey=<secure-api-key>
SmsProvider__ApiKey=<secure-api-key>
```

### Secure Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Use environment variable or Azure Key Vault"
  },
  "EmailProvider": {
    "ApiKey": "Use environment variable or Azure Key Vault"
  },
  "SmsProvider": {
    "ApiKey": "Use environment variable or Azure Key Vault"
  }
}
```

## Monitoring & Alerts

### Security Events to Monitor
- Failed login attempts (>5 in 10 minutes)
- Unauthorized access attempts
- Session hijacking attempts
- Unusual data access patterns
- System configuration changes
- User privilege escalations

### Alert Thresholds
- **Critical**: Immediate notification
  - Multiple failed logins
  - Unauthorized admin access
  - Data breach detected
  
- **Warning**: Review within 24 hours
  - Unusual access patterns
  - Session anomalies
  - Configuration changes

## Incident Response

### Security Breach Protocol

1. **Detect**: Automated monitoring alerts
2. **Contain**: Disable affected accounts
3. **Investigate**: Review audit logs
4. **Notify**: Inform affected users (POPIA requirement)
5. **Remediate**: Fix vulnerability
6. **Document**: Record incident details
7. **Review**: Update security measures

### Contact Information
- Security Team: security@ruralhealthcare.org.za
- POPIA Officer: privacy@ruralhealthcare.org.za
- Emergency: +27 XXX XXX XXXX

## Regular Security Tasks

### Daily
- Review failed login attempts
- Check system health
- Monitor active sessions

### Weekly
- Review audit logs
- Check for security updates
- Verify backup integrity

### Monthly
- User access audit
- Security patch updates
- Penetration testing
- Review security policies

### Quarterly
- Full security audit
- POPIA compliance review
- Update security documentation
- Staff security training

## Additional Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [POPIA Act](https://popia.co.za/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Azure Security Best Practices](https://docs.microsoft.com/en-us/azure/security/)
