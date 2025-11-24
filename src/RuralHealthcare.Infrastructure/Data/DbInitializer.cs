using Microsoft.EntityFrameworkCore;
using RuralHealthcare.Core.Entities;

namespace RuralHealthcare.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.Migrate();

        // Fallback: ensure required tables exist in dev when migrations are missing
        try
        {
            context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS PasswordResetTokens (
                Id TEXT NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
                UserId TEXT NOT NULL,
                Token TEXT NOT NULL,
                ExpiresAt TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UsedAt TEXT NULL,
                CONSTRAINT FK_PasswordResetTokens_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
            );");

            context.Database.ExecuteSqlRaw(@"CREATE UNIQUE INDEX IF NOT EXISTS IX_PasswordResetTokens_Token ON PasswordResetTokens (Token);");
            context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS IX_PasswordResetTokens_ExpiresAt ON PasswordResetTokens (ExpiresAt);");
            context.Database.ExecuteSqlRaw(@"CREATE INDEX IF NOT EXISTS IX_PasswordResetTokens_UserId ON PasswordResetTokens (UserId);");

            // Ensure Clinics table has AvailableMedications column (SQLite-compatible)
            try
            {
                context.Database.ExecuteSqlRaw("ALTER TABLE Clinics ADD COLUMN AvailableMedications TEXT");
            }
            catch { }
            try
            {
                context.Database.ExecuteSqlRaw("UPDATE Clinics SET AvailableMedications = '[]' WHERE AvailableMedications IS NULL");
            }
            catch { }
            try
            {
                context.Database.ExecuteSqlRaw("UPDATE Clinics SET AvailableMedications = '' WHERE AvailableMedications = '[]'");
            }
            catch { }

            // Ensure Clinics table has MedicationStock column (SQLite-compatible)
            try
            {
                context.Database.ExecuteSqlRaw("ALTER TABLE Clinics ADD COLUMN MedicationStock TEXT");
            }
            catch { }
            try
            {
                context.Database.ExecuteSqlRaw("UPDATE Clinics SET MedicationStock = '' WHERE MedicationStock IS NULL");
            }
            catch { }

            // Normalize improperly stored MedicationStock values (legacy JSON arrays)
            try
            {
                var clinicsToFix = context.Clinics.ToList();
                foreach (var c in clinicsToFix)
                {
                    var stock = c.MedicationStock ?? new List<ClinicMedicationItem>();
                    if (stock.Count == 1 && stock[0].Name != null && stock[0].Name.Contains("["))
                    {
                        var meds = c.AvailableMedications ?? new List<string>();
                        c.MedicationStock = meds.Select(n => new ClinicMedicationItem
                        {
                            Name = n,
                            Category = string.Empty,
                            InStock = true,
                            Quantity = 10,
                            LowThreshold = 2
                        }).ToList();
                        context.Update(c);
                    }
                }
                context.SaveChanges();
            }
            catch { }
        }
        catch { }

        var hasUsers = context.Users.Any();

        // Create default system admin
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@ruralhealthcare.org.za",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "System",
            LastName = "Administrator",
            Role = "system_admin",
            PhoneNumber = "+27123456789",
            IsActive = true,
            RequirePasswordChange = false, // For demo purposes
            CreatedAt = DateTime.UtcNow
        };

        if (!hasUsers)
        {
            context.Users.Add(adminUser);
        }

        // Create sample clinic
        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = "Soweto Community Clinic",
            PhoneNumber = "+27119876543",
            Address = "123 Main Road, Soweto, Johannesburg, 1804",
            Latitude = -26.2678,
            Longitude = 27.8585,
            OperatingHours = "Mon-Fri: 8AM-5PM, Sat: 8AM-1PM",
            HasAmbulance = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var existingClinic = context.Clinics.FirstOrDefault();
        if (existingClinic == null)
        {
            context.Clinics.Add(clinic);
            existingClinic = clinic;
        }
        else
        {
            clinic = existingClinic;
        }

        var soweto = context.Clinics.FirstOrDefault(c => c.Name == "Soweto Community Clinic");
        if (soweto != null)
        {
            var stockItems = new List<ClinicMedicationItem>
            {
                new ClinicMedicationItem { Name = "PrEP (TDF/FTC)", Category = "HIV / AIDS", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "ART (TDF/3TC/DTG)", Category = "HIV / AIDS", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "PEP", Category = "HIV / AIDS", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "Amlodipine 10mg", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "Metformin 500mg", Category = "Diabetes / Endocrine", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "TB Treatment (RHZE)", Category = "Infections / Antibiotics", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "Depo-Provera", Category = "Family Planning", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "Combined Oral Contraceptive", Category = "Family Planning", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "Emergency Contraceptive (Levonorgestrel)", Category = "Family Planning", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "Amoxicillin", Category = "Infections / Antibiotics", InStock = true, Quantity = 10, LowThreshold = 2 },
                new ClinicMedicationItem { Name = "Paracetamol", Category = "Pain / Analgesics", InStock = true, Quantity = 10, LowThreshold = 2 }
            };
            soweto.MedicationStock = stockItems;
            context.Update(soweto);
            context.SaveChanges();
        }

        // Create sample patient
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            IdNumber = "8501015800080",
            FirstName = "Thabo",
            LastName = "Mokoena",
            DateOfBirth = new DateTime(1985, 1, 1),
            PhoneNumber = "+27821234567",
            PreferredLanguage = "en",
            HomeAddress = "456 Township Street, Soweto",
            Latitude = -26.2700,
            Longitude = 27.8600,
            ChronicConditions = new List<string> { "Diabetes Type 2", "Hypertension" },
            Allergies = new List<string> { "Penicillin" },
            BloodType = "O+",
            EmergencyContactName = "Nomsa Mokoena",
            EmergencyContactPhone = "+27829876543",
            CreatedAt = DateTime.UtcNow,
            ClinicId = clinic.Id
        };

        if (!context.Patients.Any())
        {
            context.Patients.Add(patient);
        }

        context.SaveChanges();

        var clinicAdminEmail = "clinic.admin@ruralhealthcare.org.za";
        if (!context.Users.Any(u => u.Email == clinicAdminEmail))
        {
            var clinicAdmin = new User
            {
                Id = Guid.NewGuid(),
                Email = clinicAdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "Clinic",
                LastName = "Admin",
                Role = "clinic_admin",
                PhoneNumber = "+27110000000",
                ClinicId = clinic.Id,
                IsActive = true,
                RequirePasswordChange = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(clinicAdmin);
        }

        for (int i = 0; i < 3; i++)
        {
            var email = $"worker{i+1}@ruralhealthcare.org.za";
            if (!context.Users.Any(u => u.Email == email))
            {
                var worker = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Worker@123"),
                    FirstName = "Health",
                    LastName = $"Worker{i+1}",
                    Role = "health_worker",
                    PhoneNumber = "+27119999999",
                    ClinicId = clinic.Id,
                    IsActive = true,
                    RequirePasswordChange = false,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(worker);
            }
        }

        var rng = new Random(42);
        var basePatients = context.Patients.Where(p => p.ClinicId == clinic.Id).ToList();
        var needCount = 10 - basePatients.Count;
        if (needCount > 0)
        {
            for (int i = 0; i < needCount; i++)
            {
                var p = new Patient
                {
                    Id = Guid.NewGuid(),
                    IdNumber = $"90010{i:D2}58000{i:D2}",
                    FirstName = i % 2 == 0 ? "Lerato" : "Sibusiso",
                    LastName = i % 3 == 0 ? "Dlamini" : (i % 3 == 1 ? "Nkosi" : "Mthembu"),
                    DateOfBirth = new DateTime(1990 + (i % 10), 1 + (i % 12), 1 + (i % 28)),
                    PhoneNumber = $"+2782{rng.Next(1000000, 9999999)}",
                    PreferredLanguage = "en",
                    HomeAddress = "Soweto",
                    Latitude = -26.27,
                    Longitude = 27.86,
                    ChronicConditions = new List<string> { "Hypertension" },
                    Allergies = new List<string> { },
                    BloodType = "O+",
                    EmergencyContactName = "Contact",
                    EmergencyContactPhone = "+27820000000",
                    CreatedAt = DateTime.UtcNow,
                    ClinicId = clinic.Id
                };
                context.Patients.Add(p);
                basePatients.Add(p);
            }
            context.SaveChanges();
        }

        var medsExisting = context.Medications.Any(m => m.Patient.ClinicId == clinic.Id);
        if (!medsExisting)
        {
            foreach (var p in basePatients)
            {
                var med = new Medication
                {
                    Id = Guid.NewGuid(),
                    PatientId = p.Id,
                    Name = "Metformin",
                    Dosage = "500mg",
                    Frequency = "twice_daily",
                    ReminderTimes = new List<TimeOnly> { new TimeOnly(8,0), new TimeOnly(20,0) },
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = null,
                    IsActive = true,
                    Instructions = "Take with meals",
                    CreatedAt = DateTime.UtcNow
                };
                context.Medications.Add(med);
            }
            context.SaveChanges();
        }

        var logsExisting = context.MedicationLogs.Any(l => l.Medication.Patient.ClinicId == clinic.Id);
        if (!logsExisting)
        {
            var meds = context.Medications.Where(m => m.Patient.ClinicId == clinic.Id).ToList();
            foreach (var med in meds)
            {
                for (int d = 0; d < 30; d++)
                {
                    var day = DateTime.Today.AddDays(-d);
                    foreach (var t in med.ReminderTimes)
                    {
                        var scheduled = new DateTime(day.Year, day.Month, day.Day, t.Hour, t.Minute, 0);
                        var taken = rng.NextDouble() < 0.8;
                        var log = new MedicationLog
                        {
                            Id = Guid.NewGuid(),
                            MedicationId = med.Id,
                            ScheduledTime = scheduled,
                            TakenTime = taken ? scheduled.AddMinutes(rng.Next(0, 60)) : null,
                            WasTaken = taken,
                            Notes = taken ? null : "Missed",
                            RecordedByUserId = null
                        };
                        context.MedicationLogs.Add(log);
                    }
                }
            }
            context.SaveChanges();
        }

        var apptExisting = context.Appointments.Any(a => a.ClinicId == clinic.Id);
        if (!apptExisting)
        {
            var apptTypes = new[] { "checkup", "follow_up", "immunization", "pharmacy" };
            foreach (var p in basePatients)
            {
                for (int k = 0; k < 3; k++)
                {
                    var dt = DateTime.Today.AddDays(-rng.Next(0, 6)).AddHours(rng.Next(8, 16));
                    var statusPick = rng.Next(0, 3);
                    var status = statusPick == 0 ? "scheduled" : (statusPick == 1 ? "completed" : "missed");
                    var type = apptTypes[rng.Next(0, apptTypes.Length)];
                    var appt = new Appointment
                    {
                        Id = Guid.NewGuid(),
                        PatientId = p.Id,
                        ClinicId = clinic.Id,
                        ScheduledDateTime = dt,
                        AppointmentType = type,
                        Status = status,
                        Notes = null,
                        CompletedAt = status == "completed" ? dt.AddHours(1) : null,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Appointments.Add(appt);
                }
            }
            context.SaveChanges();
        }

        var emergExisting = context.EmergencyCalls.Any(e => e.Patient != null && e.Patient.ClinicId == clinic.Id);
        if (!emergExisting)
        {
            var emergTypes = new[] { "medical", "security", "accident" };
            foreach (var p in basePatients.Take(5))
            {
                var dt = DateTime.Today.AddDays(-rng.Next(0, 6)).AddHours(rng.Next(0, 23));
                var statusPick = rng.Next(0, 4);
                var status = statusPick == 0 ? "pending" : (statusPick == 1 ? "dispatched" : (statusPick == 2 ? "arrived" : "completed"));
                var eType = emergTypes[rng.Next(0, emergTypes.Length)];
                var call = new EmergencyCall
                {
                    Id = Guid.NewGuid(),
                    PatientId = p.Id,
                    EmergencyType = eType,
                    CallTime = dt,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    LocationDescription = null,
                    Status = status,
                    Description = null,
                    AssignedAmbulanceId = null,
                    DispatchedAt = status != "pending" ? dt.AddMinutes(10) : null,
                    ArrivedAt = status == "arrived" || status == "completed" ? dt.AddMinutes(20) : null,
                    CompletedAt = status == "completed" ? dt.AddMinutes(40) : null,
                    Patient = p
                };
                context.EmergencyCalls.Add(call);
            }
            context.SaveChanges();
        }

        if (clinic.AvailableMedications == null || clinic.AvailableMedications.Count == 0)
        {
            clinic.AvailableMedications = new List<string>
            {
                "PrEP (TDF/FTC)",
                "ART (TDF/3TC/DTG)",
                "PEP",
                "Amlodipine 10mg",
                "Metformin 500mg",
                "TB Treatment (RHZE)",
                "Depo-Provera",
                "Combined Oral Contraceptive",
                "Emergency Contraceptive (Levonorgestrel)",
                "Amoxicillin",
                "Paracetamol"
            };
            context.SaveChanges();
        }

        // Initialize MedicationStock from SA categories if missing
        if (clinic.MedicationStock == null || clinic.MedicationStock.Count == 0)
        {
            clinic.MedicationStock = new List<ClinicMedicationItem>
            {
                new ClinicMedicationItem { Name = "Dolutegravir (DTG)", Category = "HIV / AIDS", InStock = true, Quantity = 100, LowThreshold = 20 },
                new ClinicMedicationItem { Name = "TDF/3TC/DTG", Category = "HIV / AIDS", InStock = true, Quantity = 80, LowThreshold = 20 },
                new ClinicMedicationItem { Name = "Amlodipine", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 120, LowThreshold = 30 },
                new ClinicMedicationItem { Name = "Enalapril", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 60, LowThreshold = 15 },
                new ClinicMedicationItem { Name = "Hydrochlorothiazide", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 75, LowThreshold = 15 },
                new ClinicMedicationItem { Name = "Atorvastatin", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 50, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Metformin", Category = "Diabetes / Endocrine", InStock = true, Quantity = 150, LowThreshold = 40 },
                new ClinicMedicationItem { Name = "Insulin", Category = "Diabetes / Endocrine", InStock = false, Quantity = 0, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Amoxicillin", Category = "Infections / Antibiotics", InStock = true, Quantity = 90, LowThreshold = 20 },
                new ClinicMedicationItem { Name = "Metronidazole", Category = "Infections / Antibiotics", InStock = true, Quantity = 90, LowThreshold = 20 },
                new ClinicMedicationItem { Name = "Salbutamol Inhaler", Category = "Respiratory", InStock = true, Quantity = 40, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Beclomethasone Inhaler", Category = "Respiratory", InStock = false, Quantity = 0, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Fluoxetine", Category = "Mental Health", InStock = true, Quantity = 30, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Haloperidol", Category = "Mental Health", InStock = false, Quantity = 0, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Paracetamol", Category = "Pain / Analgesics", InStock = true, Quantity = 200, LowThreshold = 50 },
                new ClinicMedicationItem { Name = "Ibuprofen", Category = "Pain / Analgesics", InStock = true, Quantity = 120, LowThreshold = 30 },
                new ClinicMedicationItem { Name = "Depo-Provera", Category = "Family Planning", InStock = true, Quantity = 50, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Combined Oral Contraceptive", Category = "Family Planning", InStock = true, Quantity = 100, LowThreshold = 20 },
                new ClinicMedicationItem { Name = "IUD (Copper)", Category = "Family Planning", InStock = false, Quantity = 0, LowThreshold = 5 },
                new ClinicMedicationItem { Name = "Emergency Contraceptive (Levonorgestrel)", Category = "Family Planning", InStock = true, Quantity = 40, LowThreshold = 10 }
            };
            context.SaveChanges();
        }

        if (context.Clinics.Count() < 2)
        {
            var clinic2 = new Clinic
            {
                Id = Guid.NewGuid(),
                Name = "Alexandra Community Clinic",
                PhoneNumber = "+27115555555",
                Address = "45 1st Avenue, Alexandra, Johannesburg",
                Latitude = -26.1050,
                Longitude = 28.1070,
                OperatingHours = "Mon-Sun: 8AM-6PM",
                HasAmbulance = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Clinics.Add(clinic2);
            context.SaveChanges();

            var clinic2AdminEmail = "clinic2.admin@ruralhealthcare.org.za";
            if (!context.Users.Any(u => u.Email == clinic2AdminEmail))
            {
                var clinic2Admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = clinic2AdminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FirstName = "Clinic",
                    LastName = "Admin2",
                    Role = "clinic_admin",
                    PhoneNumber = "+27116666666",
                    ClinicId = clinic2.Id,
                    IsActive = true,
                    RequirePasswordChange = false,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(clinic2Admin);
            }

            var patients2 = new List<Patient>();
            for (int i = 0; i < 8; i++)
            {
                var p2 = new Patient
                {
                    Id = Guid.NewGuid(),
                    IdNumber = $"88020{i:D2}58000{i:D2}",
                    FirstName = i % 2 == 0 ? "Ayanda" : "Kabelo",
                    LastName = i % 3 == 0 ? "Ndlovu" : (i % 3 == 1 ? "Sithole" : "Maponya"),
                    DateOfBirth = new DateTime(1988 + (i % 12), 2 + (i % 10), 1 + (i % 28)),
                    PhoneNumber = $"+2783{rng.Next(1000000, 9999999)}",
                    PreferredLanguage = "en",
                    HomeAddress = "Alexandra",
                    Latitude = -26.106,
                    Longitude = 28.108,
                    ChronicConditions = new List<string> { "Diabetes Type 2" },
                    Allergies = new List<string> { },
                    BloodType = "A+",
                    EmergencyContactName = "Contact",
                    EmergencyContactPhone = "+27830000000",
                    CreatedAt = DateTime.UtcNow,
                    ClinicId = clinic2.Id
                };
                context.Patients.Add(p2);
                patients2.Add(p2);
            }
            context.SaveChanges();

            foreach (var p2 in patients2)
            {
                var med2 = new Medication
                {
                    Id = Guid.NewGuid(),
                    PatientId = p2.Id,
                    Name = "Amlodipine",
                    Dosage = "10mg",
                    Frequency = "daily",
                    ReminderTimes = new List<TimeOnly> { new TimeOnly(9,0) },
                    StartDate = DateTime.Today.AddDays(-30),
                    EndDate = null,
                    IsActive = true,
                    Instructions = "Take in morning",
                    CreatedAt = DateTime.UtcNow
                };
                context.Medications.Add(med2);
            }
            context.SaveChanges();

            var meds2 = context.Medications.Where(m => m.Patient.ClinicId == clinic2.Id).ToList();
            foreach (var med2 in meds2)
            {
                for (int d = 0; d < 30; d++)
                {
                    var day2 = DateTime.Today.AddDays(-d);
                    foreach (var t2 in med2.ReminderTimes)
                    {
                        var scheduled2 = new DateTime(day2.Year, day2.Month, day2.Day, t2.Hour, t2.Minute, 0);
                        var taken2 = rng.NextDouble() < 0.7;
                        var log2 = new MedicationLog
                        {
                            Id = Guid.NewGuid(),
                            MedicationId = med2.Id,
                            ScheduledTime = scheduled2,
                            TakenTime = taken2 ? scheduled2.AddMinutes(rng.Next(0, 90)) : null,
                            WasTaken = taken2,
                            Notes = taken2 ? null : "Missed",
                            RecordedByUserId = null
                        };
                        context.MedicationLogs.Add(log2);
                    }
                }
            }
            context.SaveChanges();

            var apptTypes = new[] { "checkup", "follow_up", "immunization", "pharmacy" };
            foreach (var p2 in patients2)
            {
                for (int k = 0; k < 2; k++)
                {
                    var dt2 = DateTime.Today.AddDays(-rng.Next(0, 6)).AddHours(rng.Next(8, 16));
                    var statusPick2 = rng.Next(0, 3);
                    var status2 = statusPick2 == 0 ? "scheduled" : (statusPick2 == 1 ? "completed" : "missed");
                    var type2 = apptTypes[rng.Next(0, apptTypes.Length)];
                    var appt2 = new Appointment
                    {
                        Id = Guid.NewGuid(),
                        PatientId = p2.Id,
                        ClinicId = clinic2.Id,
                        ScheduledDateTime = dt2,
                        AppointmentType = type2,
                        Status = status2,
                        Notes = null,
                        CompletedAt = status2 == "completed" ? dt2.AddHours(1) : null,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Appointments.Add(appt2);
                }
            }
            context.SaveChanges();

            var emergTypes = new[] { "medical", "security", "accident" };
            foreach (var p2 in patients2.Take(3))
            {
                var dt2 = DateTime.Today.AddDays(-rng.Next(0, 6)).AddHours(rng.Next(0, 23));
                var statusPickE2 = rng.Next(0, 4);
                var statusE2 = statusPickE2 == 0 ? "pending" : (statusPickE2 == 1 ? "dispatched" : (statusPickE2 == 2 ? "arrived" : "completed"));
                var eType2 = emergTypes[rng.Next(0, emergTypes.Length)];
                var call2 = new EmergencyCall
                {
                    Id = Guid.NewGuid(),
                    PatientId = p2.Id,
                    EmergencyType = eType2,
                    CallTime = dt2,
                    Latitude = p2.Latitude,
                    Longitude = p2.Longitude,
                    LocationDescription = null,
                    Status = statusE2,
                    Description = null,
                    AssignedAmbulanceId = null,
                    DispatchedAt = statusE2 != "pending" ? dt2.AddMinutes(10) : null,
                    ArrivedAt = statusE2 == "arrived" || statusE2 == "completed" ? dt2.AddMinutes(20) : null,
                    CompletedAt = statusE2 == "completed" ? dt2.AddMinutes(40) : null,
                    Patient = p2
                };
                context.EmergencyCalls.Add(call2);
            }
            context.SaveChanges();

            clinic2.AvailableMedications = new List<string>
            {
                "PrEP (TDF/FTC)",
                "ART (AZT/3TC/DTG)",
                "PEP",
                "Amlodipine 10mg",
                "Insulin",
                "TB Treatment (RHZE)",
                "IUD (Copper)",
                "Emergency Contraceptive (Levonorgestrel)",
                "Amoxicillin",
                "Paracetamol"
            };
            context.SaveChanges();

            // Initialize MedicationStock for clinic2
            clinic2.MedicationStock = new List<ClinicMedicationItem>
            {
                new ClinicMedicationItem { Name = "Dolutegravir (DTG)", Category = "HIV / AIDS", InStock = true, Quantity = 60, LowThreshold = 15 },
                new ClinicMedicationItem { Name = "AZT/3TC/DTG", Category = "HIV / AIDS", InStock = true, Quantity = 70, LowThreshold = 15 },
                new ClinicMedicationItem { Name = "Amlodipine", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 90, LowThreshold = 25 },
                new ClinicMedicationItem { Name = "Enalapril", Category = "Hypertension / Cardiovascular", InStock = false, Quantity = 0, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Hydrochlorothiazide", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 60, LowThreshold = 15 },
                new ClinicMedicationItem { Name = "Simvastatin", Category = "Hypertension / Cardiovascular", InStock = true, Quantity = 40, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Metformin", Category = "Diabetes / Endocrine", InStock = true, Quantity = 120, LowThreshold = 30 },
                new ClinicMedicationItem { Name = "Insulin", Category = "Diabetes / Endocrine", InStock = true, Quantity = 20, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Amoxicillin", Category = "Infections / Antibiotics", InStock = true, Quantity = 80, LowThreshold = 20 },
                new ClinicMedicationItem { Name = "Metronidazole", Category = "Infections / Antibiotics", InStock = false, Quantity = 0, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Salbutamol Inhaler", Category = "Respiratory", InStock = true, Quantity = 35, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Beclomethasone Inhaler", Category = "Respiratory", InStock = true, Quantity = 20, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Fluoxetine", Category = "Mental Health", InStock = true, Quantity = 25, LowThreshold = 8 },
                new ClinicMedicationItem { Name = "Haloperidol", Category = "Mental Health", InStock = false, Quantity = 0, LowThreshold = 8 },
                new ClinicMedicationItem { Name = "Paracetamol", Category = "Pain / Analgesics", InStock = true, Quantity = 150, LowThreshold = 40 },
                new ClinicMedicationItem { Name = "Ibuprofen", Category = "Pain / Analgesics", InStock = true, Quantity = 100, LowThreshold = 25 },
                new ClinicMedicationItem { Name = "Depo-Provera", Category = "Family Planning", InStock = true, Quantity = 40, LowThreshold = 10 },
                new ClinicMedicationItem { Name = "Combined Oral Contraceptive", Category = "Family Planning", InStock = true, Quantity = 80, LowThreshold = 20 },
                new ClinicMedicationItem { Name = "IUD (Copper)", Category = "Family Planning", InStock = true, Quantity = 10, LowThreshold = 3 },
                new ClinicMedicationItem { Name = "Emergency Contraceptive (Levonorgestrel)", Category = "Family Planning", InStock = true, Quantity = 30, LowThreshold = 10 }
            };
            context.SaveChanges();
        }

        Console.WriteLine("Database seeded successfully!");
        Console.WriteLine("=================================");
        Console.WriteLine("Default Admin Credentials:");
        Console.WriteLine("Email: admin@ruralhealthcare.org.za");
        Console.WriteLine("Password: Admin@123");
        Console.WriteLine("=================================");
    }
}
