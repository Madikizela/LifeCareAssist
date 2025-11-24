# System Administrator Guide

## Overview

The System Admin role has full access to manage the entire Rural Healthcare Platform, including:
- Clinics and hospitals
- System users (admins, health workers, caregivers)
- Patients (view all)
- Emergency services
- System configuration
- Reports and analytics

## Getting Started

### First Login
1. Navigate to `/admin/dashboard`
2. Login with system admin credentials
3. Review system health status

## Managing Clinics & Hospitals

### Adding a New Clinic

1. Go to **Admin → Clinics**
2. Click **"+ Add New Clinic"**
3. Fill in required information:
   - Facility name
   - Phone number
   - Physical address
   - GPS coordinates (use "Get Current Location" or enter manually)
   - Operating hours
   - Ambulance availability
4. Click **"Save Clinic"**

### GPS Location Tips
- **Use Current Location**: If you're at the facility, click this button
- **Get from Address**: Uses geocoding to find coordinates (requires API setup)
- **Manual Entry**: Use Google Maps to find exact coordinates
  - Right-click on location → "What's here?"
  - Copy latitude and longitude

### Clinic Capabilities
- **Ambulance Service**: Check if facility has ambulance for emergency dispatch
- **Active Status**: Only active clinics appear in patient/emergency workflows

## Managing Users

### User Roles

#### System Admin
- Full system access
- Manage all clinics and users
- View all patients
- System configuration
- Reports and analytics

#### Clinic Admin
- Manage assigned clinic
- View clinic patients
- Manage clinic staff
- Clinic-level reports

#### Health Worker
- Home visits
- Patient check-ins
- Medication delivery
- Symptom reporting
- Referrals to clinic

#### Caregiver
- View assigned patients
- Receive medication alerts
- Emergency notifications
- Limited patient information

### Adding a New User

1. Go to **Admin → Users**
2. Click **"+ Add New User"**
3. Fill in details:
   - Name and email
   - Phone number
   - Role
   - Clinic assignment (if applicable)
   - Temporary password
4. Click **"Create User"**

**Note**: User will be prompted to change password on first login.

### User Management Best Practices

- **Unique Emails**: Each user must have unique email address
- **Strong Passwords**: Enforce minimum 8 characters, mixed case, numbers
- **Clinic Assignment**: 
  - Clinic Admins must be assigned to a clinic
  - Health Workers should be assigned to their primary clinic
  - System Admins don't need clinic assignment
- **Regular Audits**: Review user list monthly, deactivate unused accounts

## Dashboard Overview

### Key Metrics
- **Total Patients**: All registered patients in system
- **Active Clinics**: Operational facilities
- **Health Workers**: Active field staff
- **Emergency Calls (24h)**: Recent emergency activity

### System Health Indicators
- **Database**: Connection and performance status
- **SMS Service**: Notification service status
- **Reminder Service**: Background job status

### Recent Activity
- Real-time feed of system events
- Patient registrations
- Emergency calls
- Clinic additions

## Emergency Management

### Monitoring Active Emergencies
1. Dashboard shows 24-hour emergency count
2. Click to view all active emergencies
3. Track status: Pending → Dispatched → Arrived → Completed

### Emergency Response Workflow
1. Patient/caregiver triggers emergency
2. System captures GPS location
3. Finds nearest clinic with ambulance
4. Notifies emergency contact
5. Tracks response time

## Reports & Analytics

### Available Reports
- **Medication Adherence**: Overall and per-patient rates
- **Emergency Response Times**: Average dispatch and arrival times
- **Clinic Performance**: Patient load, appointment attendance
- **Health Worker Activity**: Home visits, patient check-ins
- **Geographic Coverage**: Heat maps of patient distribution

### Generating Reports
1. Go to **Admin → Reports**
2. Select report type
3. Choose date range
4. Apply filters (clinic, region, etc.)
5. Export as PDF or Excel

## System Configuration

### SMS Provider Setup
1. Go to **Admin → Settings → SMS**
2. Choose provider (Clickatell, Twilio, BulkSMS)
3. Enter API credentials
4. Test connection
5. Configure message templates

### Language Settings
- Default language: English
- Supported: isiZulu, isiXhosa, Sesotho, Setswana
- Translation management for SMS templates

### Notification Settings
- Medication reminder times
- Appointment reminder lead time (default: 24 hours)
- Emergency alert recipients
- Escalation rules for missed medications

## Security & Compliance

### POPIA Compliance
- Patient data encryption at rest
- Audit logs for all access
- Patient consent management
- Data retention policies

### Access Control
- Role-based permissions
- Multi-factor authentication (recommended)
- Session timeout: 30 minutes
- Password expiry: 90 days

### Audit Logs
- All user actions logged
- Patient data access tracked
- Export logs for compliance
- Retention: 7 years

## Troubleshooting

### Common Issues

#### SMS Not Sending
1. Check SMS provider status in dashboard
2. Verify API credentials in settings
3. Check account balance
4. Review error logs

#### Emergency Calls Not Dispatching
1. Verify clinic has ambulance enabled
2. Check GPS coordinates are valid
3. Ensure clinic is marked as active
4. Review emergency call logs

#### Users Can't Login
1. Verify user is active
2. Check email is correct
3. Reset password if needed
4. Check role permissions

### Getting Help
- Check system logs: `/admin/logs`
- Review error messages
- Contact technical support
- Community forum (if available)

## Best Practices

### Daily Tasks
- Review emergency calls from previous 24 hours
- Check system health indicators
- Monitor medication adherence alerts
- Review new patient registrations

### Weekly Tasks
- Review user activity logs
- Check clinic status and updates
- Generate weekly reports
- Review and respond to alerts

### Monthly Tasks
- User access audit
- System performance review
- Backup verification
- Update documentation
- Staff training sessions

## Maintenance Windows

### Scheduled Maintenance
- Typically: Sunday 2AM-4AM SAST
- Notifications sent 48 hours in advance
- Emergency services remain operational
- SMS fallback activated

### Emergency Maintenance
- Rare, only for critical issues
- Minimal downtime
- Status updates via SMS

## Support Contacts

### Technical Support
- Email: support@ruralhealthcare.org.za
- Phone: +27 XXX XXX XXXX
- Hours: 24/7 for emergencies

### Training & Onboarding
- Email: training@ruralhealthcare.org.za
- Schedule: Monday-Friday 8AM-5PM

## Appendix

### Keyboard Shortcuts
- `Ctrl + K`: Quick search
- `Ctrl + D`: Dashboard
- `Ctrl + P`: Patients
- `Ctrl + E`: Emergency

### API Access
- Documentation: `/api/docs`
- API Key management: `/admin/api-keys`
- Rate limits: 1000 requests/hour
