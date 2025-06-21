#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ManaxApi.Migrations.Read;

/// <inheritdoc />
public partial class ReadV01 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Chapter",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FileName = table.Column<string>("TEXT", nullable: false),
                Number = table.Column<int>("INTEGER", nullable: false),
                Pages = table.Column<int>("INTEGER", nullable: false),
                Path = table.Column<string>("TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Chapter", x => x.Id); });

        migrationBuilder.CreateTable(
            "User",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Username = table.Column<string>("TEXT", maxLength: 50, nullable: false),
                PasswordHash = table.Column<string>("TEXT", maxLength: 128, nullable: false),
                Role = table.Column<int>("INTEGER", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_User", x => x.Id); });

        migrationBuilder.CreateTable(
            "Reads",
            table => new
            {
                ChapterId = table.Column<long>("INTEGER", nullable: false),
                UserId = table.Column<long>("INTEGER", nullable: false),
                Date = table.Column<DateTime>("TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reads", x => new { x.ChapterId, x.UserId });
                table.ForeignKey(
                    "FK_Reads_Chapter_ChapterId",
                    x => x.ChapterId,
                    "Chapter",
                    "Id");
                table.ForeignKey(
                    "FK_Reads_User_UserId",
                    x => x.UserId,
                    "User",
                    "Id");
            });

        migrationBuilder.CreateIndex(
            "IX_Reads_UserId",
            "Reads",
            "UserId");

        migrationBuilder.CreateIndex(
            "IX_User_Username",
            "User",
            "Username",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Reads");

        migrationBuilder.DropTable(
            "Chapter");

        migrationBuilder.DropTable(
            "User");
    }
}