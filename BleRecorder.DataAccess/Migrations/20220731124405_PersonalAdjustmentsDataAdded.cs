using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    public partial class PersonalAdjustmentsDataAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomizedAdjustmentsId",
                table: "TestSubjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomizedParametersId",
                table: "TestSubjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "TestSubjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AdjustmentsDuringMeasurementId",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParametersDuringMeasurementId",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PositionDuringMeasurement",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SiteDuringMeasurement",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DeviceMechanicalAdjustments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AnkleAxisX = table.Column<int>(type: "INTEGER", nullable: false),
                    AnkleAxisY = table.Column<int>(type: "INTEGER", nullable: false),
                    AnkleAxisZ = table.Column<int>(type: "INTEGER", nullable: false),
                    TibiaLength = table.Column<int>(type: "INTEGER", nullable: false),
                    KneeAxisDeviation = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMechanicalAdjustments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestSubjects_CustomizedAdjustmentsId",
                table: "TestSubjects",
                column: "CustomizedAdjustmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSubjects_CustomizedParametersId",
                table: "TestSubjects",
                column: "CustomizedParametersId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_AdjustmentsDuringMeasurementId",
                table: "Measurements",
                column: "AdjustmentsDuringMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_ParametersDuringMeasurementId",
                table: "Measurements",
                column: "ParametersDuringMeasurementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_DeviceMechanicalAdjustments_AdjustmentsDuringMeasurementId",
                table: "Measurements",
                column: "AdjustmentsDuringMeasurementId",
                principalTable: "DeviceMechanicalAdjustments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_StimulationParameters_ParametersDuringMeasurementId",
                table: "Measurements",
                column: "ParametersDuringMeasurementId",
                principalTable: "StimulationParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestSubjects_DeviceMechanicalAdjustments_CustomizedAdjustmentsId",
                table: "TestSubjects",
                column: "CustomizedAdjustmentsId",
                principalTable: "DeviceMechanicalAdjustments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestSubjects_StimulationParameters_CustomizedParametersId",
                table: "TestSubjects",
                column: "CustomizedParametersId",
                principalTable: "StimulationParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_DeviceMechanicalAdjustments_AdjustmentsDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_StimulationParameters_ParametersDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropForeignKey(
                name: "FK_TestSubjects_DeviceMechanicalAdjustments_CustomizedAdjustmentsId",
                table: "TestSubjects");

            migrationBuilder.DropForeignKey(
                name: "FK_TestSubjects_StimulationParameters_CustomizedParametersId",
                table: "TestSubjects");

            migrationBuilder.DropTable(
                name: "DeviceMechanicalAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_TestSubjects_CustomizedAdjustmentsId",
                table: "TestSubjects");

            migrationBuilder.DropIndex(
                name: "IX_TestSubjects_CustomizedParametersId",
                table: "TestSubjects");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_AdjustmentsDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_ParametersDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "CustomizedAdjustmentsId",
                table: "TestSubjects");

            migrationBuilder.DropColumn(
                name: "CustomizedParametersId",
                table: "TestSubjects");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "TestSubjects");

            migrationBuilder.DropColumn(
                name: "AdjustmentsDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "ParametersDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "PositionDuringMeasurement",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "SiteDuringMeasurement",
                table: "Measurements");
        }
    }
}
