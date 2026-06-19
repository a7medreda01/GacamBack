using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppDAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsAndCertificateDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateDesigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecondaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BorderColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BorderWidth = table.Column<float>(type: "real", nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeaderTextEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HeaderTextAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SignatoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignatoryTitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignatoryTitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SignatureImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShowLogo = table.Column<bool>(type: "bit", nullable: false),
                    LogoHeight = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateDesigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteTitleEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteTitleAr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocialLinksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateDesigns");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
