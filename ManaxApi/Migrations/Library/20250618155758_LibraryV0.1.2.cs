using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManaxApi.Migrations
{
    /// <inheritdoc />
    public partial class LibraryV012 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_LibraryInfo_LibraryInfoId",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Serie");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Serie");

            migrationBuilder.RenameColumn(
                name: "LibraryInfoId",
                table: "Libraries",
                newName: "InfosId");

            migrationBuilder.RenameIndex(
                name: "IX_Libraries_LibraryInfoId",
                table: "Libraries",
                newName: "IX_Libraries_InfosId");

            migrationBuilder.AddColumn<long>(
                name: "InfosId",
                table: "Serie",
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
                name: "IX_Serie_InfosId",
                table: "Serie",
                column: "InfosId");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_LibraryInfo_InfosId",
                table: "Libraries",
                column: "InfosId",
                principalTable: "LibraryInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Serie_SerieInfo_InfosId",
                table: "Serie",
                column: "InfosId",
                principalTable: "SerieInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_LibraryInfo_InfosId",
                table: "Libraries");

            migrationBuilder.DropForeignKey(
                name: "FK_Serie_SerieInfo_InfosId",
                table: "Serie");

            migrationBuilder.DropTable(
                name: "SerieInfo");

            migrationBuilder.DropIndex(
                name: "IX_Serie_InfosId",
                table: "Serie");

            migrationBuilder.DropColumn(
                name: "InfosId",
                table: "Serie");

            migrationBuilder.RenameColumn(
                name: "InfosId",
                table: "Libraries",
                newName: "LibraryInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Libraries_InfosId",
                table: "Libraries",
                newName: "IX_Libraries_LibraryInfoId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Serie",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Serie",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_LibraryInfo_LibraryInfoId",
                table: "Libraries",
                column: "LibraryInfoId",
                principalTable: "LibraryInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
