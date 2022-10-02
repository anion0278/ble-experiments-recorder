using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebster.Myodam.DataAccess.Migrations
{
    public partial class AddedFatigueStimParams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "FatigueRepetitions", "RestTime", "StimulationTime" },
                values: new object[] { 4, new TimeSpan(0, 0, 0, 5, 0), new TimeSpan(0, 0, 0, 5, 0) });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FatigueRepetitions",
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
