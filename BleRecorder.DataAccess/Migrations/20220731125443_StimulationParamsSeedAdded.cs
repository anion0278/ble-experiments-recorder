using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    public partial class StimulationParamsSeedAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "StimulationParameters",
                columns: new[] { "Id", "Current", "Frequency", "PulseWidth", "StimulationTime" },
                values: new object[] { 1, 10, 50, 50, new TimeSpan(0, 0, 0, 5, 0) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
