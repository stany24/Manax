using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManaxApi.Migrations.Serie
{
    /// <inheritdoc />
    public partial class SerieV011 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Series");

            migrationBuilder.AddColumn<long>(
                name: "InfosId",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "SerieInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerieInfo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Series_InfosId",
                table: "Series",
                column: "InfosId");

            migrationBuilder.AddForeignKey(
                name: "FK_Series_SerieInfo_InfosId",
                table: "Series",
                column: "InfosId",
                principalTable: "SerieInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Series_SerieInfo_InfosId",
                table: "Series");

            migrationBuilder.DropTable(
                name: "SerieInfo");

            migrationBuilder.DropIndex(
                name: "IX_Series_InfosId",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "InfosId",
                table: "Series");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Series",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Series",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
