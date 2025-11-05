using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuntoryManagementSystem_Models.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToAllModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStock",
                table: "StockAlerts");

            migrationBuilder.DropColumn(
                name: "MinimumStock",
                table: "StockAlerts");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Suppliers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AlertType",
                table: "StockAlerts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "StockAlerts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StockAlerts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "StockAdjustments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StockAdjustments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "DeliveryItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DeliveryItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Deliveries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Deliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "AlertType",
                table: "StockAlerts");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "StockAlerts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockAlerts");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "StockAdjustments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockAdjustments");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "DeliveryItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DeliveryItems");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Deliveries");

            migrationBuilder.AddColumn<int>(
                name: "CurrentStock",
                table: "StockAlerts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimumStock",
                table: "StockAlerts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
