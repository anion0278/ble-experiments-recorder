using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    public partial class AddedIntermittentStimParams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IntermittentRepetitions",
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

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IntermittentRepetitions", "RestTime", "StimulationTime" },
                values: new object[] { 4, new TimeSpan(0, 0, 0, 5, 0), new TimeSpan(0, 0, 0, 5, 0) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntermittentRepetitions",
                table: "StimulationParameters");

            migrationBuilder.DropColumn(
                name: "RestTime",
                table: "StimulationParameters");

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "StimulationTime",
                value: new TimeSpan(0, 0, 0, 10, 0));
        }
    }
}
