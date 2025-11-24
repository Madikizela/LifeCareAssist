# Authentication & User Management

## Overview

The Rural Healthcare Platform uses email-based authentication with auto-generated secure passwords. All users receive their credentials via email and must change their password on first login.

## User Creation Flow

### 1. Admin Creates User
1. System Admin navigates to **Admin → Users → Add New User**
2. Fills in user details:
   - First Name
   - Last Name
   - Email (used as username)
   - Phone Number (optional)
   - Role
   - Clinic Assignment (if applicable)
3. Clicks **"Create User"**

### 2. System Generates Password
- Secure 12-character password automatically generated
- Contains:
  - Uppercase letters (A-Z)
  - Lowercase letters (a-z)
  - Numbers (0-9)
  - Special characters (!@#$%^&*)
- Uses cryptographically secure random number generator

### 3. Email Sent to User
Email contains:
- Username (their email address)
- Temporary password
- Login URL
- Instructions to change password on first login

**Example Email:**
```
Subject: Welcome to Rural Healthcare Platform

Hello John,

Your account has been created successfully. Please use the following credentials to login:

Username (Email): john.doe@clinic.org.za
Temporary Password: Xy9$mK2@pL4n

Important: You will be required to change your password on first login.

Login URL: https://yourapp.com/login
```

## Login Process

### First Login
1. User navigates to `/login`
2. Enters email and temporary password
3. System validates credentials
4. User is redirected to `/change-password`
5. Must create new secure password
6. Redirected to appropriate dashboard based on role

### Subsequent Logins
1. User enters email and new password
2. System validates credentials
3. Redirected to role-specific dashboard:
   - **System Admin** → `/admin/dashboard`
   - **Clinic Admin** → `/admin/dashboard`
   - **Health Worker** → `/patients`
   - **Caregiver** → `/`

## Password Requirements

### Minimum Requirements
- At least 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character (!@#$%^&*()_+-=[]{}|;:,.<>?)

### Password Change Rules
- Must be different from current password
- Cannot reuse last 5 passwords (future enhancement)
- Expires every 90 days (future enhancement)

## Security Features

### Password Hashing
- Uses BCrypt with salt
- Computationally expensive to prevent brute force
- Industry-standard security

### Session Management
- 30-minute idle timeout
- HttpOnly cookies (prevents XSS attacks)
- Secure flag in production (HTTPS only)

### Account Security
- Email uniqueness enforced
- Account lockout after 5 failed attempts (future enhancement)
- Password reset via email
- Audit log of all login attempts

## User Roles & Permissions

### System Admin
- Full system access
- Manage all users and clinics
- View all patients
- System configuration
- Reports and analytics

### Clinic Admin
- Manage assigned clinic
- View clinic patients
- Manage clinic staff
- Clinic-level reports
- Cannot access other clinics

### Health Worker
- View assigned patients
- Record home visits
- Update patient information
- Submit referrals
- Cannot manage users

### Caregiver
- View assigned patients only
- Receive medication alerts
- View appointment schedules
- Cannot edit patient data

## Email Configuration

### Supported Providers

#### SendGrid (Recommended)
```json
{
  "EmailProvider": {
    "Provider": "SendGrid",
    "ApiKey": "SG.xxxxxxxxxxxxx",
    "FromEmail": "noreply@ruralhealthcare.org.za",
    "FromName": "Rural Healthcare Platform"
  }
}
```

#### SMTP (Generic)
```json
{
  "EmailProvider": {
    "Provider": "SMTP",
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@ruralhealthcare.org.za",
    "FromName": "Rural Healthcare Platform"
  }
}
```

#### Azure Communication Services
```json
{
  "EmailProvider": {
    "Provider": "AzureEmail",
    "ConnectionString": "endpoint=https://xxx.communication.azure.com/;accesskey=xxx",
    "FromEmail": "noreply@ruralhealthcare.org.za"
  }
}
```

## Troubleshooting

### User Didn't Receive Email

**Check:**
1. Email provider configuration in `appsettings.json`
2. Spam/junk folder
3. Email provider logs
4. Correct email address entered

**Solution:**
- Admin can view temporary password in success message
- Manually provide credentials to user
- Resend email from user management page

### User Can't Login

**Check:**
1. Email address is correct (case-sensitive)
2. Password is correct (case-sensitive)
3. Account is active
4. No typos in credentials

**Solution:**
- Reset password via "Forgot Password" link
- Admin can reset password from user management
- Check account status in admin panel

### Password Change Failed

**Check:**
1. Current password is correct
2. New password meets requirements
3. New password matches confirmation

**Solution:**
- Review password requirements
- Try different password
- Contact admin if issue persists

## Best Practices

### For Admins
- Use strong, unique passwords
- Enable 2FA when available
- Regularly review user accounts
- Deactivate unused accounts
- Monitor login attempts

### For Users
- Change temporary password immediately
- Don't share credentials
- Use password manager
- Log out when finished
- Report suspicious activity

### For System
- Regular security audits
- Keep software updated
- Monitor failed login attempts
- Review access logs
- Backup user data

## Future Enhancements

### Planned Features
- Two-factor authentication (2FA)
- Single Sign-On (SSO)
- Biometric login (mobile app)
- Password expiry policies
- Account lockout after failed attempts
- Password history (prevent reuse)
- Security questions
- Login notifications
- Device management

### Integration Options
- Azure Active Directory
- Google Workspace
- SAML 2.0
- OAuth 2.0
- OpenID Connect

## API Authentication

### For Mobile Apps
```csharp
POST /api/auth/login
{
  "email": "user@example.com",
  "password": "password123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600,
  "requirePasswordChange": false
}
```

### JWT Token
- Expires in 1 hour
- Refresh token valid for 7 days
- Include in Authorization header: `Bearer {token}`

## Compliance

### POPIA Requirements
- User consent for data processing
- Right to access personal data
- Right to delete account
- Data breach notification
- Audit trail of access

### Security Standards
- OWASP Top 10 compliance
- ISO 27001 alignment
- HIPAA-like protections
- Regular penetration testing
