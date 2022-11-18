using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebster.Myodam.DataAccess.Migrations
{
    public partial class AddedDeviceCalibration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_DeviceMechanicalAdjustments_AdjustmentsDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_StimulationParameters_ParametersDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.AlterColumn<int>(
                name: "ParametersDuringMeasurementId",
                table: "Measurements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "AdjustmentsDuringMeasurementId",
                table: "Measurements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_DeviceMechanicalAdjustments_AdjustmentsDuringMeasurementId",
                table: "Measurements",
                column: "AdjustmentsDuringMeasurementId",
                principalTable: "DeviceMechanicalAdjustments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_StimulationParameters_ParametersDuringMeasurementId",
                table: "Measurements",
                column: "ParametersDuringMeasurementId",
                principalTable: "StimulationParameters",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_DeviceMechanicalAdjustments_AdjustmentsDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_StimulationParameters_ParametersDuringMeasurementId",
                table: "Measurements");

            migrationBuilder.DropTable(
                name: "DeviceCalibrations");

            migrationBuilder.AlterColumn<int>(
                name: "ParametersDuringMeasurementId",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AdjustmentsDuringMeasurementId",
                table: "Measurements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

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
        }
    }
}
