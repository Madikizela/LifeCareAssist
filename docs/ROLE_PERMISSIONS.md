# Role-Based Access Control (RBAC)

## Overview

The Rural Healthcare Platform implements strict role-based access control to ensure data security and proper separation of duties.

## User Roles & Permissions

### 1. System Admin

**Full System Access**

#### Can Do:
- ✅ Manage ALL clinics (create, view, edit, delete)
- ✅ Create ALL types of users (System Admin, Clinic Admin, Health Workers, Caregivers)
- ✅ View ALL patients across all clinics
- ✅ View ALL appointments across all clinics
- ✅ View ALL medications across all clinics
- ✅ View ALL emergency calls
- ✅ System-wide reports and analytics
- ✅ System configuration
- ✅ Access all admin features

#### Cannot Do:
- ❌ Nothing - has full access

---

### 2. Clinic Admin

**Clinic-Scoped Access**

#### Can Do:
- ✅ View ONLY their assigned clinic
- ✅ Edit ONLY their assigned clinic details
- ✅ Create Health Workers for their clinic
- ✅ Create Caregivers for their clinic
- ✅ View patients at their clinic
- ✅ Manage appointments at their clinic
- ✅ Manage medications for their clinic patients
- ✅ View emergency calls from their area
- ✅ View clinic-specific dashboard metrics

#### Cannot Do:
- ❌ Create new clinics
- ❌ Delete their clinic
- ❌ Create System Admins
- ❌ Create other Clinic Admins
- ❌ View other clinics
- ❌ View patients from other clinics
- ❌ Access system-wide settings
- ❌ View system-wide reports

#### Restrictions:
- **Clinic Scope**: All data is filtered to show only their assigned clinic
- **User Creation**: Can only create `health_worker` and `caregiver` roles
- **Clinic Assignment**: New users are automatically assigned to their clinic
- **Dashboard**: Shows only their clinic's metrics

---

### 3. Health Worker

**Patient Care Access**

#### Can Do:
- ✅ View assigned patients
- ✅ Update patient information
- ✅ Record home visits
- ✅ Add medication logs
- ✅ Submit referrals
- ✅ Create emergency calls
- ✅ View appointments

#### Cannot Do:
- ❌ Create new patients (must be done by admin)
- ❌ Delete patients
- ❌ Create users
- ❌ Access admin dashboard
- ❌ Manage clinics
- ❌ View system reports

---

### 4. Caregiver

**Limited Patient Access**

#### Can Do:
- ✅ View assigned patients only
- ✅ Receive medication alerts
- ✅ View appointment schedules
- ✅ Create emergency calls for their patients

#### Cannot Do:
- ❌ Edit patient information
- ❌ Create medications
- ❌ Schedule appointments
- ❌ Access admin features
- ❌ View other patients

---

## Implementation Details

### Authentication Check
All protected pages check:
```csharp
var userId = HttpContext.Session.GetString("UserId");
if (string.IsNullOrEmpty(userId))
{
    return RedirectToPage("/Index"); // Redirect to login
}
```

### Authorization Check
Role-specific pages check:
```csharp
var userRole = HttpContext.Session.GetString("UserRole");
if (userRole != "system_admin" && userRole != "clinic_admin")
{
    return RedirectToPage("/AccessDenied");
}
```

### Data Filtering (Clinic Admin)
```csharp
if (userRole == "clinic_admin")
{
    var user = await _context.Users.FindAsync(Guid.Parse(userId));
    if (user?.ClinicId.HasValue == true)
    {
        query = query.Where(c => c.Id == user.ClinicId.Value);
    }
}
```

---

## Access Matrix

| Feature | System Admin | Clinic Admin | Health Worker | Caregiver |
|---------|-------------|--------------|---------------|-----------|
| **Clinics** |
| View All Clinics | ✅ | ❌ | ❌ | ❌ |
| View Own Clinic | ✅ | ✅ | ❌ | ❌ |
| Create Clinic | ✅ | ❌ | ❌ | ❌ |
| Edit Any Clinic | ✅ | ❌ | ❌ | ❌ |
| Edit Own Clinic | ✅ | ✅ | ❌ | ❌ |
| **Users** |
| Create System Admin | ✅ | ❌ | ❌ | ❌ |
| Create Clinic Admin | ✅ | ❌ | ❌ | ❌ |
| Create Health Worker | ✅ | ✅ | ❌ | ❌ |
| Create Caregiver | ✅ | ✅ | ❌ | ❌ |
| View All Users | ✅ | ❌ | ❌ | ❌ |
| View Clinic Users | ✅ | ✅ | ❌ | ❌ |
| **Patients** |
| View All Patients | ✅ | ❌ | ❌ | ❌ |
| View Clinic Patients | ✅ | ✅ | ✅ | ❌ |
| View Assigned Patients | ✅ | ✅ | ✅ | ✅ |
| Create Patient | ✅ | ✅ | ❌ | ❌ |
| Edit Patient | ✅ | ✅ | ✅ | ❌ |
| **Appointments** |
| View All Appointments | ✅ | ❌ | ❌ | ❌ |
| View Clinic Appointments | ✅ | ✅ | ✅ | ❌ |
| Create Appointment | ✅ | ✅ | ✅ | ❌ |
| **Medications** |
| View All Medications | ✅ | ❌ | ❌ | ❌ |
| View Clinic Medications | ✅ | ✅ | ✅ | ❌ |
| Create Medication | ✅ | ✅ | ✅ | ❌ |
| Log Medication Taken | ✅ | ✅ | ✅ | ✅ |
| **Emergency** |
| View All Emergencies | ✅ | ❌ | ❌ | ❌ |
| View Clinic Emergencies | ✅ | ✅ | ✅ | ❌ |
| Create Emergency Call | ✅ | ✅ | ✅ | ✅ |
| **Dashboard** |
| System Dashboard | ✅ | ❌ | ❌ | ❌ |
| Clinic Dashboard | ✅ | ✅ | ❌ | ❌ |

---

## Testing Role Restrictions

### Test as System Admin
1. Login as: admin@ruralhealthcare.org.za
2. Can see all clinics
3. Can create any type of user
4. Can access all features

### Test as Clinic Admin
1. Login as: Madikizela21517799@gmail.com
2. Can only see Soweto Community Clinic
3. Can only create Health Workers and Caregivers
4. Cannot create new clinics
5. Cannot see other clinics' data

### Test as Health Worker
1. Create a health worker user
2. Can view and manage patients
3. Cannot access admin dashboard
4. Cannot create users

---

## Security Notes

- All role checks happen server-side
- Session-based authentication
- 30-minute session timeout
- Automatic redirect on unauthorized access
- Audit logging of all access attempts

---

## Future Enhancements

- [ ] Patient assignment to specific health workers
- [ ] Caregiver assignment to specific patients
- [ ] Multi-clinic access for health workers
- [ ] Temporary access grants
- [ ] Role-based API access tokens
- [ ] Advanced audit logging with role tracking
