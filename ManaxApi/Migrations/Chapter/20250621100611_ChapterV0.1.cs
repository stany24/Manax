#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ManaxApi.Migrations.Chapter;

/// <inheritdoc />
public partial class ChapterV01 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Chapters",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FileName = table.Column<string>("TEXT", nullable: false),
                Number = table.Column<int>("INTEGER", nullable: false),
                Pages = table.Column<int>("INTEGER", nullable: false),
                Path = table.Column<string>("TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Chapters", x => x.Id); });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Chapters");
    }
}