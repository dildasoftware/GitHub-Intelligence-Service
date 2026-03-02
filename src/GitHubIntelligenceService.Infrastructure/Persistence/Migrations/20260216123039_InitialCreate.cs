using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitHubIntelligenceService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Developers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Bio = table.Column<string>(type: "TEXT", nullable: true),
                    Score_TotalScore = table.Column<double>(type: "REAL", nullable: false),
                    Score_ActivityScore = table.Column<double>(type: "REAL", nullable: false),
                    Score_QualityScore = table.Column<double>(type: "REAL", nullable: false),
                    Score_PopularityScore = table.Column<double>(type: "REAL", nullable: false),
                    Score_SeniorityLevel = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeveloperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Repository_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Repository_Description = table.Column<string>(type: "TEXT", nullable: true),
                    Repository_Language = table.Column<string>(type: "TEXT", nullable: true),
                    Repository_Stars = table.Column<int>(type: "INTEGER", nullable: false),
                    Repository_Forks = table.Column<int>(type: "INTEGER", nullable: false),
                    Repository_IssuesCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Repository_CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Repository_LastPushedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Score = table.Column<double>(type: "REAL", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Stars = table.Column<int>(type: "INTEGER", nullable: false),
                    Issues = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Repositories_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_DeveloperId",
                table: "Repositories",
                column: "DeveloperId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "Developers");
        }
    }
}
