using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuntoryManagementSystem_Models.Migrations.SuntoryDb
{
    /// <inheritdoc />
    public partial class FixForeignKeyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Customers_CustomerId1",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId1",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Vehicles_VehicleId1",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId1",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SupplierId1",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_CustomerId1",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_SupplierId1",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_VehicleId1",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "SupplierId1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "SupplierId1",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "VehicleId1",
                table: "Deliveries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId1",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId1",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId1",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId1",
                table: "Deliveries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SupplierId1",
                table: "Products",
                column: "SupplierId1");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_CustomerId1",
                table: "Deliveries",
                column: "CustomerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_SupplierId1",
                table: "Deliveries",
                column: "SupplierId1");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_VehicleId1",
                table: "Deliveries",
                column: "VehicleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Customers_CustomerId1",
                table: "Deliveries",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId1",
                table: "Deliveries",
                column: "SupplierId1",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Vehicles_VehicleId1",
                table: "Deliveries",
                column: "VehicleId1",
                principalTable: "Vehicles",
                principalColumn: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId1",
                table: "Products",
                column: "SupplierId1",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");
        }
    }
}
