using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    public partial class ChangedParamNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Current",
                table: "StimulationParameters",
                newName: "Amplitude");

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "IntermittentStimulationTime",
                value: new TimeSpan(0, 0, 0, 1, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amplitude",
                table: "StimulationParameters",
                newName: "Current");

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "IntermittentStimulationTime",
                value: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
