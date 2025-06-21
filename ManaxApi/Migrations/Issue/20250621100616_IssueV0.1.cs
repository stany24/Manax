#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ManaxApi.Migrations.Issue;

/// <inheritdoc />
public partial class IssueV01 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
            "Issues",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserId = table.Column<long>("INTEGER", nullable: false),
                Problem = table.Column<string>("TEXT", maxLength: 128, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Issues", x => x.Id);
                table.ForeignKey(
                    "FK_Issues_User_UserId",
                    x => x.UserId,
                    "User",
                    "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            "Chapter",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FileName = table.Column<string>("TEXT", nullable: false),
                Number = table.Column<int>("INTEGER", nullable: false),
                Pages = table.Column<int>("INTEGER", nullable: false),
                Path = table.Column<string>("TEXT", nullable: false),
                IssueId = table.Column<long>("INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Chapter", x => x.Id);
                table.ForeignKey(
                    "FK_Chapter_Issues_IssueId",
                    x => x.IssueId,
                    "Issues",
                    "Id");
            });

        migrationBuilder.CreateIndex(
            "IX_Chapter_IssueId",
            "Chapter",
            "IssueId");

        migrationBuilder.CreateIndex(
            "IX_Issues_UserId",
            "Issues",
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
            "Chapter");

        migrationBuilder.DropTable(
            "Issues");

        migrationBuilder.DropTable(
            "User");
    }
}