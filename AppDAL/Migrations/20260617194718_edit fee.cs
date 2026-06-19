using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppDAL.Migrations
{
    /// <inheritdoc />
    public partial class editfee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceFees_Code",
                table: "ServiceFees");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ServiceFees");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ServiceFees");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "ServiceFees");

            migrationBuilder.RenameColumn(
                name: "ProcessingFee",
                table: "ServiceFees",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ServiceFees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "OrderType",
                table: "ServiceFees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ServiceFees");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "ServiceFees");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "ServiceFees",
                newName: "ProcessingFee");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ServiceFees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ServiceFees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "ServiceFees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceFees_Code",
                table: "ServiceFees",
                column: "Code",
                unique: true);
        }
    }
}
