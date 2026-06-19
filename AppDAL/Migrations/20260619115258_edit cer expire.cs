using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppDAL.Migrations
{
    /// <inheritdoc />
    public partial class editcerexpire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Certificates_ExpiredAt",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "Certificates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "Certificates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ExpiredAt",
                table: "Certificates",
                column: "ExpiredAt");
        }
    }
}
