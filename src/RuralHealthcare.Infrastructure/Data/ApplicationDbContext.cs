using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RuralHealthcare.Core.Entities;
using System.Text.Json;

namespace RuralHealthcare.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationLog> MedicationLogs => Set<MedicationLog>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<EmergencyCall> EmergencyCalls => Set<EmergencyCall>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdNumber).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.Property(e => e.ChronicConditions).HasColumnType("jsonb");
            entity.Property(e => e.Allergies).HasColumnType("jsonb");
            entity.HasOne(e => e.Clinic)
                  .WithMany(c => c.Patients)
                  .HasForeignKey(e => e.ClinicId);
            entity.HasIndex(e => e.ClinicId);
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Patient).WithMany(p => p.Medications).HasForeignKey(e => e.PatientId);
            entity.Property(e => e.ReminderTimes).HasColumnType("jsonb");
        });

        modelBuilder.Entity<MedicationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Medication).WithMany(m => m.Logs).HasForeignKey(e => e.MedicationId);
            entity.HasIndex(e => new { e.MedicationId, e.ScheduledTime });
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Patient).WithMany(p => p.Appointments).HasForeignKey(e => e.PatientId);
            entity.HasOne(e => e.Clinic).WithMany(c => c.Appointments).HasForeignKey(e => e.ClinicId);
            entity.HasIndex(e => e.ScheduledDateTime);
        });

        modelBuilder.Entity<EmergencyCall>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.EmergencyCalls)
                .HasForeignKey(e => e.PatientId)
                .IsRequired(false); // Allow anonymous emergency calls
            entity.HasIndex(e => e.CallTime);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CallerPhone);
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            var converter = new ValueConverter<List<string>, string>(
                v => v == null ? string.Empty : string.Join("||", v),
                v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split("||", StringSplitOptions.RemoveEmptyEntries).ToList()
            );
            
            entity.Property(e => e.AvailableMedications)
                  .HasConversion(converter)
                  .HasColumnType("TEXT")
                  ;

            var stockConverter = new ValueConverter<List<ClinicMedicationItem>, string>(
                v => ClinicStockConverterUtil.Serialize(v),
                v => ClinicStockConverterUtil.Deserialize(v)
            );

            entity.Property(e => e.MedicationStock)
                  .HasConversion(stockConverter)
                  .HasColumnType("TEXT")
                  ;
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

static class ClinicStockConverterUtil
{
    public static string Serialize(List<ClinicMedicationItem> v)
        => v == null ? string.Empty : string.Join("||", v.Select(i => ($"{i.Name}::{i.Category}::{(i.InStock ? 1 : 0)}::{i.Quantity}::{i.LowThreshold}")));

    public static List<ClinicMedicationItem> Deserialize(string v)
        => string.IsNullOrEmpty(v)
            ? new List<ClinicMedicationItem>()
            : v.Split("||", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => {
                    var parts = s.Split("::", StringSplitOptions.None);
                    var name = parts.Length > 0 ? parts[0] : string.Empty;
                    var category = parts.Length > 1 ? parts[1] : string.Empty;
                    var flag = parts.Length > 2 && parts[2] == "1";
                    var qty = parts.Length > 3 ? (int.TryParse(parts[3], out var q) ? q : 0) : 0;
                    var thr = parts.Length > 4 ? (int.TryParse(parts[4], out var t) ? t : 0) : 0;
                    return new ClinicMedicationItem { Name = name, Category = category, InStock = flag, Quantity = qty, LowThreshold = thr };
                })
                .ToList();
}
