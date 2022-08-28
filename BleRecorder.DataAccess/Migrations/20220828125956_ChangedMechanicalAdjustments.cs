using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    public partial class ChangedMechanicalAdjustments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnkleAxisX",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.DropColumn(
                name: "AnkleAxisY",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.DropColumn(
                name: "AnkleAxisZ",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.DropColumn(
                name: "KneeAxisDeviation",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.DropColumn(
                name: "TibiaLength",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.AddColumn<double>(
                name: "CuffProximalDistalDistance",
                table: "DeviceMechanicalAdjustments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FixtureAdductionAbductionAngle",
                table: "DeviceMechanicalAdjustments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FixtureAnteroPosteriorDistance",
                table: "DeviceMechanicalAdjustments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FixtureProximalDistalDistance",
                table: "DeviceMechanicalAdjustments",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "StimulationTime",
                value: new TimeSpan(0, 0, 0, 10, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuffProximalDistalDistance",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.DropColumn(
                name: "FixtureAdductionAbductionAngle",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.DropColumn(
                name: "FixtureAnteroPosteriorDistance",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.DropColumn(
                name: "FixtureProximalDistalDistance",
                table: "DeviceMechanicalAdjustments");

            migrationBuilder.AddColumn<int>(
                name: "AnkleAxisX",
                table: "DeviceMechanicalAdjustments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnkleAxisY",
                table: "DeviceMechanicalAdjustments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AnkleAxisZ",
                table: "DeviceMechanicalAdjustments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KneeAxisDeviation",
                table: "DeviceMechanicalAdjustments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TibiaLength",
                table: "DeviceMechanicalAdjustments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "StimulationParameters",
                keyColumn: "Id",
                keyValue: 1,
                column: "StimulationTime",
                value: new TimeSpan(0, 0, 0, 5, 0));
        }
    }
}
