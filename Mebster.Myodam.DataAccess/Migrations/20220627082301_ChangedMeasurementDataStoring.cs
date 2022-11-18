using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebster.Myodam.DataAccess.Migrations
{
    public partial class ChangedMeasurementDataStoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InternalForceData",
                table: "Measurements",
                newName: "ForceData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ForceData",
                table: "Measurements",
                newName: "InternalForceData");
        }
    }
}
