using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuralHealthcare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportAnonymousEmergencyCalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyCalls_Patients_PatientId",
                table: "EmergencyCalls");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "EmergencyCalls",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "CallerIdNumber",
                table: "EmergencyCalls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallerName",
                table: "EmergencyCalls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallerPhone",
                table: "EmergencyCalls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationDescription",
                table: "EmergencyCalls",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyCalls_CallerPhone",
                table: "EmergencyCalls",
                column: "CallerPhone");

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyCalls_Patients_PatientId",
                table: "EmergencyCalls",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmergencyCalls_Patients_PatientId",
                table: "EmergencyCalls");

            migrationBuilder.DropIndex(
                name: "IX_EmergencyCalls_CallerPhone",
                table: "EmergencyCalls");

            migrationBuilder.DropColumn(
                name: "CallerIdNumber",
                table: "EmergencyCalls");

            migrationBuilder.DropColumn(
                name: "CallerName",
                table: "EmergencyCalls");

            migrationBuilder.DropColumn(
                name: "CallerPhone",
                table: "EmergencyCalls");

            migrationBuilder.DropColumn(
                name: "LocationDescription",
                table: "EmergencyCalls");

            migrationBuilder.AlterColumn<Guid>(
                name: "PatientId",
                table: "EmergencyCalls",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmergencyCalls_Patients_PatientId",
                table: "EmergencyCalls",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
