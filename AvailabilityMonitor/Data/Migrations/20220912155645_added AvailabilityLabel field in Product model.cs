using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvailabilityMonitor.Data.Migrations
{
    public partial class addedAvailabilityLabelfieldinProductmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityLabel",
                table: "Product",
                type: "nvarchar(40)",
                nullable: true,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityLabel",
                table: "Product");
        }
    }
}
