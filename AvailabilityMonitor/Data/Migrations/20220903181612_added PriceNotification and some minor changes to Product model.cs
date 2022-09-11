using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvailabilityMonitor.Data.Migrations
{
    public partial class addedPriceNotificationandsomeminorchangestoProductmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "supplierRetailPrice",
                table: "Product",
                newName: "SupplierRetailPrice");

            migrationBuilder.RenameColumn(
                name: "supplierQuantity",
                table: "Product",
                newName: "SupplierQuantity");

            migrationBuilder.RenameColumn(
                name: "stockavailableId",
                table: "Product",
                newName: "StockavailableId");

            migrationBuilder.RenameColumn(
                name: "retailPrice",
                table: "Product",
                newName: "RetailPrice");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "Product",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "prestashopId",
                table: "Product",
                newName: "PrestashopId");

            migrationBuilder.RenameColumn(
                name: "photoURL",
                table: "Product",
                newName: "PhotoURL");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Product",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "isVisible",
                table: "Product",
                newName: "IsVisible");

            migrationBuilder.RenameColumn(
                name: "index",
                table: "Product",
                newName: "Index");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Product",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "wholesalePrice",
                table: "Product",
                newName: "SupplierWholesalePrice");

            migrationBuilder.AlterColumn<float>(
                name: "RetailPrice",
                table: "Product",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SupplierRetailPrice",
                table: "Product",
                newName: "supplierRetailPrice");

            migrationBuilder.RenameColumn(
                name: "SupplierQuantity",
                table: "Product",
                newName: "supplierQuantity");

            migrationBuilder.RenameColumn(
                name: "StockavailableId",
                table: "Product",
                newName: "stockavailableId");

            migrationBuilder.RenameColumn(
                name: "RetailPrice",
                table: "Product",
                newName: "retailPrice");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "Product",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "PrestashopId",
                table: "Product",
                newName: "prestashopId");

            migrationBuilder.RenameColumn(
                name: "PhotoURL",
                table: "Product",
                newName: "photoURL");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Product",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "IsVisible",
                table: "Product",
                newName: "isVisible");

            migrationBuilder.RenameColumn(
                name: "Index",
                table: "Product",
                newName: "index");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Product",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SupplierWholesalePrice",
                table: "Product",
                newName: "wholesalePrice");

            migrationBuilder.AlterColumn<double>(
                name: "retailPrice",
                table: "Product",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");
        }
    }
}
