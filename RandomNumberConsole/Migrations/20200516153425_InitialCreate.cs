using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RandomNumberConsole.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RandomNumbers",
                columns: table => new
                {
                    RandomNumberId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<int>(nullable: false),
                    GeneratedTime = table.Column<DateTime>(nullable: false),
                    ServiceCallRetryRequired = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PrimaryKey_RandomNumberId", x => x.RandomNumberId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RandomNumbers");
        }
    }
}
