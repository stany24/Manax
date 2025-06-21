#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ManaxApi.Migrations;

/// <inheritdoc />
public partial class UserV01 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Users",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Username = table.Column<string>("TEXT", maxLength: 50, nullable: false),
                PasswordHash = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                Role = table.Column<int>("INTEGER", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

        migrationBuilder.CreateIndex(
            "IX_Users_Username",
            "Users",
            "Username",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Users");
    }
}