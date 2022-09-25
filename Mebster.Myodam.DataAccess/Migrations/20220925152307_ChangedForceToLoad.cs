using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebster.Myodam.DataAccess.Migrations
{
    public partial class ChangedForceToLoad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceCalibrations");

            migrationBuilder.RenameColumn(
                name: "ForceData",
                table: "Measurements",
                newName: "ContractionLoadData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContractionLoadData",
                table: "Measurements",
                newName: "ForceData");

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
