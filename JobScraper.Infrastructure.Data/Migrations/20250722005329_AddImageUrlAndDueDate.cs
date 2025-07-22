using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobScraper.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlAndDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company",
                table: "JobListings");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Skills",
                newName: "EnglishName");

            migrationBuilder.RenameIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                newName: "IX_Skills_EnglishName");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Skills",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "KoreanName",
                table: "Skills",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "JobListings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "JobDetails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "JobDetails",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "JobDetails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "JobDetails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "LocationLatitude",
                table: "JobDetails",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LocationLongitude",
                table: "JobDetails",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredQualifications",
                table: "JobDetails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "JobDetails",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    EstablishedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_JobListings_CompanyId",
                table: "JobListings",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobListings_Companies_CompanyId",
                table: "JobListings",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobListings_Companies_CompanyId",
                table: "JobListings");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_JobListings_CompanyId",
                table: "JobListings");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "KoreanName",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "JobListings");

            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "JobDetails");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "JobDetails");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "JobDetails");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "JobDetails");

            migrationBuilder.DropColumn(
                name: "LocationLatitude",
                table: "JobDetails");

            migrationBuilder.DropColumn(
                name: "LocationLongitude",
                table: "JobDetails");

            migrationBuilder.DropColumn(
                name: "PreferredQualifications",
                table: "JobDetails");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "JobDetails");

            migrationBuilder.RenameColumn(
                name: "EnglishName",
                table: "Skills",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Skills_EnglishName",
                table: "Skills",
                newName: "IX_Skills_Name");

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "JobListings",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
