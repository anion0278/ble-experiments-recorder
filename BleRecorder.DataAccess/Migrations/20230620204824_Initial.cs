using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceMechanicalAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixtureAdductionAbductionAngle = table.Column<double>(type: "float", nullable: false),
                    FixtureProximalDistalDistance = table.Column<double>(type: "float", nullable: false),
                    FixtureAnteroPosteriorDistance = table.Column<double>(type: "float", nullable: false),
                    CuffProximalDistalDistance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMechanicalAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StimulationParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amplitude = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    PulseWidth = table.Column<int>(type: "int", nullable: false),
                    StimulationTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IntermittentStimulationTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    RestTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IntermittentRepetitions = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StimulationParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomizedAdjustmentsId = table.Column<int>(type: "int", nullable: false),
                    CustomizedParametersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestSubjects_DeviceMechanicalAdjustments_CustomizedAdjustmentsId",
                        column: x => x.CustomizedAdjustmentsId,
                        principalTable: "DeviceMechanicalAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestSubjects_StimulationParameters_CustomizedParametersId",
                        column: x => x.CustomizedParametersId,
                        principalTable: "StimulationParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PositionDuringMeasurement = table.Column<int>(type: "int", nullable: false),
                    SiteDuringMeasurement = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TestSubjectId = table.Column<int>(type: "int", nullable: false),
                    AdjustmentsDuringMeasurementId = table.Column<int>(type: "int", nullable: true),
                    ParametersDuringMeasurementId = table.Column<int>(type: "int", nullable: true),
                    ContractionLoadData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurements_DeviceMechanicalAdjustments_AdjustmentsDuringMeasurementId",
                        column: x => x.AdjustmentsDuringMeasurementId,
                        principalTable: "DeviceMechanicalAdjustments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Measurements_StimulationParameters_ParametersDuringMeasurementId",
                        column: x => x.ParametersDuringMeasurementId,
                        principalTable: "StimulationParameters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Measurements_TestSubjects_TestSubjectId",
                        column: x => x.TestSubjectId,
                        principalTable: "TestSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "StimulationParameters",
                columns: new[] { "Id", "Amplitude", "Frequency", "IntermittentRepetitions", "IntermittentStimulationTime", "PulseWidth", "RestTime", "StimulationTime" },
                values: new object[] { 1, 10, 50, 4, new TimeSpan(0, 0, 0, 1, 0), 50, new TimeSpan(0, 0, 0, 5, 0), new TimeSpan(0, 0, 0, 5, 0) });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_AdjustmentsDuringMeasurementId",
                table: "Measurements",
                column: "AdjustmentsDuringMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_ParametersDuringMeasurementId",
                table: "Measurements",
                column: "ParametersDuringMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_TestSubjectId",
                table: "Measurements",
                column: "TestSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSubjects_CustomizedAdjustmentsId",
                table: "TestSubjects",
                column: "CustomizedAdjustmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSubjects_CustomizedParametersId",
                table: "TestSubjects",
                column: "CustomizedParametersId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropTable(
                name: "TestSubjects");

            migrationBuilder.DropTable(
                name: "DeviceMechanicalAdjustments");

            migrationBuilder.DropTable(
                name: "StimulationParameters");
        }
    }
}
