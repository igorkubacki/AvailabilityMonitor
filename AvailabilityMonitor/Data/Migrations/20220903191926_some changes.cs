using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvailabilityMonitor.Data.Migrations
{
    public partial class somechanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isRead",
                table: "PriceNotification",
                newName: "IsRead");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "PriceNotification",
                newName: "isRead");
        }
    }
}
