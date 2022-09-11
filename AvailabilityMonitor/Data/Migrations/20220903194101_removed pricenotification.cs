using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvailabilityMonitor.Data.Migrations
{
    public partial class removedpricenotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceNotification");

            migrationBuilder.AddColumn<bool>(
                name: "IsNotificationRead",
                table: "PriceChange",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotificationRead",
                table: "PriceChange");

            migrationBuilder.CreateTable(
                name: "PriceNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceChangeId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceNotification_PriceChange_PriceChangeId",
                        column: x => x.PriceChangeId,
                        principalTable: "PriceChange",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceNotification_PriceChangeId",
                table: "PriceNotification",
                column: "PriceChangeId",
                unique: true);
        }
    }
}
