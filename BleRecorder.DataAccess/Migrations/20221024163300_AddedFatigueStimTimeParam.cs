using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    public partial class AddedIntermittentStimTimeParam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "IntermittentStimulationTime",
                table: "StimulationParameters",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntermittentStimulationTime",
                table: "StimulationParameters");
        }
    }
}
