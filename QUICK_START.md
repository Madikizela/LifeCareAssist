# 🚀 Quick Start Guide

## Application is Running!

**URL:** http://localhost:5000

## 🔐 Login Credentials

### System Administrator
- **Email:** admin@ruralhealthcare.org.za
- **Password:** Admin@123

## 📱 What You Can Do

### Landing Page (http://localhost:5000)
- ✅ View system description and features
- ✅ Login form for staff members
- ✅ Quick access to emergency services
- ✅ See platform statistics

### After Login (Admin)
- ✅ **Dashboard** - `/admin/dashboard` - System overview and metrics
- ✅ **Manage Clinics** - `/admin/clinics` - Add/edit hospitals and clinics
- ✅ **Manage Users** - `/admin/users` - Create staff accounts with auto-generated passwords
- ✅ **View Patients** - `/patients` - Patient management
- ✅ **Emergency Calls** - `/emergency` - Active emergency tracking

### Public Access (No Login Required)
- ✅ **Emergency Call** - `/emergency/create` - Anyone can call for help

## 🎯 Test Scenarios

### 1. Test User Creation
1. Login as admin
2. Go to Admin → Users → Add New User
3. Fill in details (email, name, role)
4. System generates password automatically
5. Check console for "email sent" message (shows generated password)

### 2. Test Clinic Management
1. Go to Admin → Clinics → Add New Clinic
2. Fill in clinic details
3. Use "Get Current Location" for GPS coordinates
4. Mark if ambulance is available

### 3. Test Patient Management
1. Go to Patients → Add New Patient
2. Add chronic conditions (comma-separated)
3. Add allergies
4. View patient details page

### 4. Test Emergency System
1. Go to Emergency → Create Emergency Call
2. Select patient
3. Choose emergency type (Medical/Security)
4. System captures GPS location
5. View active emergencies at `/emergency`

## 📊 Sample Data Included

### Pre-loaded Data:
- **1 Admin User** - Ready to login
- **1 Sample Clinic** - Soweto Community Clinic (with ambulance)
- **1 Sample Patient** - Thabo Mokoena (with chronic conditions)

## 🔧 Development Commands

### Stop the Application
```powershell
# Press Ctrl+C in the terminal where it's running
```

### Restart the Application
```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/RuralHealthcare.Web
```

### Rebuild CSS (if you modify Tailwind)
```powershell
cd src/RuralHealthcare.Web
npm run build:css
```

### Reset Database
```powershell
# Delete the database file
Remove-Item ruralhealthcare.db

# Run migrations again
dotnet ef database update --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web
```

## 🎨 Key Features Implemented

### ✅ Landing Page
- Hero section with login form
- System description and features
- Emergency call button
- Statistics display
- Multi-section layout (Hero, Features, Emergency, Stats, Footer)

### ✅ Authentication
- Email-based login
- Auto-generated secure passwords
- Email notifications (console output for now)
- Role-based redirects
- Session management

### ✅ Admin Features
- System dashboard with metrics
- Clinic management with GPS
- User management with roles
- Patient overview
- Emergency tracking

### ✅ Public Features
- Emergency call system (no login required)
- GPS location capture
- Patient selection
- Emergency type selection

## 🌐 Multi-Language Support

Languages configured:
- English (en)
- isiZulu (zu)
- isiXhosa (xh)
- Sesotho (st)
- Setswana (tn)

## 📧 Email Configuration

Currently emails are logged to console. To enable real emails:

1. Update `appsettings.json`:
```json
"EmailProvider": {
  "Provider": "SendGrid",
  "ApiKey": "your_actual_sendgrid_api_key",
  "FromEmail": "noreply@ruralhealthcare.org.za",
  "FromName": "Rural Healthcare Platform"
}
```

2. Implement actual email sending in `EmailService.cs`

## 🗄️ Database

- **Type:** SQLite (Development)
- **File:** `ruralhealthcare.db` (in project root)
- **Production:** PostgreSQL (configured in appsettings.json)

## 🚀 Next Steps

1. **Test all features** on the landing page
2. **Create additional users** with different roles
3. **Add more clinics** in your area
4. **Register patients** and test medication tracking
5. **Configure SMS provider** for real notifications
6. **Setup email provider** for user credentials

## 📞 Support

Check the documentation:
- `README.md` - Project overview
- `docs/SETUP.md` - Detailed setup
- `docs/ADMIN_GUIDE.md` - Admin manual
- `docs/AUTHENTICATION.md` - Login system
- `docs/FEATURES.md` - Feature details

---

**Enjoy testing your Rural Healthcare Platform! 🏥**
