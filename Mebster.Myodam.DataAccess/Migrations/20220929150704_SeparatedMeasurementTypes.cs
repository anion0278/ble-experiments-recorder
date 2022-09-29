using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebster.Myodam.DataAccess.Migrations
{
    public partial class SeparatedMeasurementTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceCalibrations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Measurements");

            migrationBuilder.RenameColumn(
                name: "ForceData",
                table: "Measurements",
                newName: "MeasurementTypeDiscriminator");

            migrationBuilder.AddColumn<int>(
                name: "FatigueRepetitions",
                table: "StimulationParameters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "RestTime",
                table: "StimulationParameters",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "MultiCycleRecord_Data",
                table: "Measurements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Record_Data",
                table: "Measurements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FatigueRepetitions", "RestTime" },
                values: new object[] { 10, new TimeSpan(0, 0, 0, 5, 0) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FatigueRepetitions",
                table: "StimulationParameters");

            migrationBuilder.DropColumn(
                name: "RestTime",
                table: "StimulationParameters");

            migrationBuilder.DropColumn(
                name: "MultiCycleRecord_Data",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "Record_Data",
                table: "Measurements");

            migrationBuilder.RenameColumn(
                name: "MeasurementTypeDiscriminator",
                table: "Measurements",
                newName: "ForceData");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DeviceCalibrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NoLoadSensorValue = table.Column<double>(type: "REAL", nullable: false),
                    NominalLoadSensorValue = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceCalibrations", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DeviceCalibrations",
                columns: new[] { "Id", "NoLoadSensorValue", "NominalLoadSensorValue" },
                values: new object[] { 1, 1.0, 1.0 });
        }
    }
}
