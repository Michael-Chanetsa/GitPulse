using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitPulse.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScanResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Bugs = table.Column<int>(type: "int", nullable: false),
                    Vulnerabilities = table.Column<int>(type: "int", nullable: false),
                    CodeSmells = table.Column<int>(type: "int", nullable: false),
                    Coverage = table.Column<double>(type: "float", nullable: true),
                    DuplicatedLinesDensity = table.Column<double>(type: "float", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QualityGate = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScanResults");
        }
    }
}
