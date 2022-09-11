using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvailabilityMonitor.Data.Migrations
{
    public partial class addedPriceNotificationpropertyinPriceChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceNotification_PriceChangeId",
                table: "PriceNotification");

            migrationBuilder.CreateIndex(
                name: "IX_PriceNotification_PriceChangeId",
                table: "PriceNotification",
                column: "PriceChangeId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceNotification_PriceChangeId",
                table: "PriceNotification");

            migrationBuilder.CreateIndex(
                name: "IX_PriceNotification_PriceChangeId",
                table: "PriceNotification",
                column: "PriceChangeId");
        }
    }
}
