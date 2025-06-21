#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ManaxApi.Migrations.Library;

/// <inheritdoc />
public partial class LibraryV01 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Libraries",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>("TEXT", nullable: false),
                Description = table.Column<string>("TEXT", nullable: false),
                Path = table.Column<string>("TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Libraries", x => x.Id); });

        migrationBuilder.CreateTable(
            "Serie",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FolderName = table.Column<string>("TEXT", nullable: false),
                Title = table.Column<string>("TEXT", nullable: false),
                Description = table.Column<string>("TEXT", nullable: false),
                LibraryId = table.Column<long>("INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Serie", x => x.Id);
                table.ForeignKey(
                    "FK_Serie_Libraries_LibraryId",
                    x => x.LibraryId,
                    "Libraries",
                    "Id");
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
                SerieId = table.Column<long>("INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Chapter", x => x.Id);
                table.ForeignKey(
                    "FK_Chapter_Serie_SerieId",
                    x => x.SerieId,
                    "Serie",
                    "Id");
            });

        migrationBuilder.CreateIndex(
            "IX_Chapter_SerieId",
            "Chapter",
            "SerieId");

        migrationBuilder.CreateIndex(
            "IX_Serie_LibraryId",
            "Serie",
            "LibraryId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Chapter");

        migrationBuilder.DropTable(
            "Serie");

        migrationBuilder.DropTable(
            "Libraries");
    }
}