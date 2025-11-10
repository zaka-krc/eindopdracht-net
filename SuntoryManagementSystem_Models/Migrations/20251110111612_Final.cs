using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuntoryManagementSystem_Models.Migrations
{
    /// <inheritdoc />
    public partial class Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Customers_CustomerId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryItems_Products_ProductId1",
                table: "DeliveryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustments_Products_ProductId",
                table: "StockAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_Products_ProductId",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryItems_ProductId1",
                table: "DeliveryItems");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "DeliveryItems");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Customers_CustomerId",
                table: "Deliveries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Customers_CustomerId1",
                table: "Deliveries",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId",
                table: "Deliveries",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId1",
                table: "Deliveries",
                column: "SupplierId1",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId1",
                table: "Products",
                column: "SupplierId1",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustments_Products_ProductId",
                table: "StockAdjustments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_Products_ProductId",
                table: "StockAlerts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Customers_CustomerId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Customers_CustomerId1",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId1",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Suppliers_SupplierId1",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustments_Products_ProductId",
                table: "StockAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAlerts_Products_ProductId",
                table: "StockAlerts");

            migrationBuilder.DropIndex(
                name: "IX_Products_SupplierId1",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_CustomerId1",
                table: "Deliveries");

            migrationBuilder.DropIndex(
                name: "IX_Deliveries_SupplierId1",
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

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "DeliveryItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryItems_ProductId1",
                table: "DeliveryItems",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Customers_CustomerId",
                table: "Deliveries",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_Suppliers_SupplierId",
                table: "Deliveries",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryItems_Products_ProductId1",
                table: "DeliveryItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Suppliers_SupplierId",
                table: "Products",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustments_Products_ProductId",
                table: "StockAdjustments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAlerts_Products_ProductId",
                table: "StockAlerts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
