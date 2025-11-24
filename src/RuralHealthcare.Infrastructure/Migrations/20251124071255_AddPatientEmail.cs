using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuralHealthcare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Reminder1DaySent",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Reminder3DaysSent",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSameDaySent",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Reminder1DaySent",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Reminder3DaysSent",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderSameDaySent",
                table: "Appointments");
        }
    }
}
