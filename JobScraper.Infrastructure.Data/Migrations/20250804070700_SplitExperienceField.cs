using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobScraper.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitExperienceField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Experience",
                table: "JobDetails",
                newName: "ExperienceMin");

            migrationBuilder.AddColumn<int>(
                name: "ExperienceMax",
                table: "JobDetails",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExperienceMax",
                table: "JobDetails");

            migrationBuilder.RenameColumn(
                name: "ExperienceMin",
                table: "JobDetails",
                newName: "Experience");
        }
    }
}
