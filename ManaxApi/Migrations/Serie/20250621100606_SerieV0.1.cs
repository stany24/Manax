#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ManaxApi.Migrations.Serie;

/// <inheritdoc />
public partial class SerieV01 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Series",
            table => new
            {
                Id = table.Column<long>("INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                FolderName = table.Column<string>("TEXT", nullable: false),
                Title = table.Column<string>("TEXT", nullable: false),
                Description = table.Column<string>("TEXT", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Series", x => x.Id); });

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
                    "FK_Chapter_Series_SerieId",
                    x => x.SerieId,
                    "Series",
                    "Id");
            });

        migrationBuilder.CreateIndex(
            "IX_Chapter_SerieId",
            "Chapter",
            "SerieId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Chapter");

        migrationBuilder.DropTable(
            "Series");
    }
}