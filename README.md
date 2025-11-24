# Rural Healthcare Platform - South Africa

A comprehensive healthcare management system designed for rural South African communities, focusing on medication reminders, emergency services, and community health worker coordination.

## 🎯 Vision

Improve healthcare access and medication adherence in rural South Africa through technology that works in low-connectivity environments with multi-language support.

## ✨ Core Features

### Phase 1 (Current)
- ✅ Patient registration & medical history
- ✅ Medication tracking with automated reminders
- ✅ Emergency ambulance dispatch system
- ✅ Multi-language support (English, isiZulu, isiXhosa, Sesotho, Setswana)
- ✅ GPS location tracking for emergencies
- ✅ Chronic condition & allergy management

### Phase 2 (Planned)
- 📱 SMS & Voice call reminders
- 🏥 Clinic appointment scheduling
- 👨‍⚕️ Community health worker app
- 📊 Admin dashboard with analytics
- 🔄 Offline mode support
- 📞 USSD integration for feature phones

### Phase 3 (Future)
- 🤖 AI health assistant
- 💊 Pharmacy integration
- 🚑 Ambulance dispatch optimization
- 📱 Mobile app (React Native/Flutter)
- 🏷️ QR code patient identification

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8.0
- **Frontend**: Razor Pages + Tailwind CSS
- **Database**: PostgreSQL 14+
- **SMS/Voice**: Clickatell/Twilio integration ready
- **Deployment**: Azure, Docker, or Linux VPS

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 14+
- Node.js 18+

### Setup
```powershell
# Run automated setup
.\setup.ps1

# Update database connection in appsettings.json
# Then run migrations
dotnet ef migrations add InitialCreate --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web
dotnet ef database update --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web

# Run application
dotnet run --project src/RuralHealthcare.Web
```

Visit: https://localhost:5001

## 📁 Project Structure

```
RuralHealthcarePlatform/
├── src/
│   ├── RuralHealthcare.Web/          # Web application (Razor Pages)
│   ├── RuralHealthcare.Core/         # Domain models & entities
│   ├── RuralHealthcare.Infrastructure/ # Data access & EF Core
│   └── RuralHealthcare.Services/     # Business logic & notifications
├── database/                          # SQL scripts & migrations
├── docs/                              # Documentation
│   ├── SETUP.md                       # Detailed setup guide
│   ├── ROADMAP.md                     # Development roadmap
│   ├── FEATURES.md                    # Feature implementation guide
│   ├── DEPLOYMENT.md                  # Production deployment
│   └── API.md                         # API documentation
└── README.md
```

## 📖 Documentation

- [Setup Guide](docs/SETUP.md) - Detailed installation instructions
- [Admin Guide](docs/ADMIN_GUIDE.md) - System administrator manual
- [Authentication Guide](docs/AUTHENTICATION.md) - User management & login system
- [Feature Guide](docs/FEATURES.md) - Implementation details for all features
- [Deployment Guide](docs/DEPLOYMENT.md) - Production deployment options
- [API Documentation](docs/API.md) - REST API endpoints
- [Roadmap](docs/ROADMAP.md) - Development timeline

## 🌍 Target Users

1. **Rural Clinics** - Patient management and medication tracking
2. **NGOs** - Community health worker coordination
3. **Patients** - Medication reminders and emergency services
4. **Health Workers** - Home visit tracking and patient monitoring
5. **Ambulance Services** - Emergency dispatch and coordination

## 💡 Key Differentiators

- **Offline-first**: Works in areas with poor connectivity
- **Multi-language**: Native language support for SA communities
- **USSD support**: Works on basic feature phones (R50 phones)
- **GPS integration**: Automatic location detection for emergencies
- **Community-focused**: Built for rural healthcare workflows

## 🔒 Security & Compliance

- POPIA (Protection of Personal Information Act) compliant
- Encrypted patient data
- Role-based access control
- Audit logging
- HTTPS enforced

## 💰 Cost Estimates

### Small Clinic (500 patients)
- Hosting: R500-1000/month
- Database: R300-500/month
- SMS: R3000-5000/month
- **Total: ~R4000-6500/month**

### Large NGO (5000 patients)
- Hosting: R2000-3000/month
- Database: R1000-1500/month
- SMS: R30000-50000/month
- **Total: ~R33000-54500/month**

## 🤝 Contributing

This project is designed to serve South African rural communities. Contributions welcome!

## 📄 License

MIT License - Free for NGOs and public health organizations

## 📞 Support

For implementation support or customization for your clinic/NGO, see documentation or open an issue.

---

**Built with ❤️ for South African rural healthcare**
