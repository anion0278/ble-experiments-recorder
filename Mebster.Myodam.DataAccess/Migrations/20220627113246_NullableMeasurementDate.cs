using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mebster.Myodam.DataAccess.Migrations
{
    public partial class NullableMeasurementDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Date",
                table: "Measurements",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Date",
                table: "Measurements",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
