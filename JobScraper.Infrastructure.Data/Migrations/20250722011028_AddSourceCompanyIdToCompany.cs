using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobScraper.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceCompanyIdToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceCompanyId",
                table: "Companies",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceCompanyId",
                table: "Companies");
        }
    }
}
