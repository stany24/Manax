using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ManaxApi.Migrations
{
    /// <inheritdoc />
    public partial class LibraryV011 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Libraries");

            migrationBuilder.AddColumn<long>(
                name: "LibraryInfoId",
                table: "Libraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "LibraryInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryInfo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_LibraryInfoId",
                table: "Libraries",
                column: "LibraryInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Libraries_LibraryInfo_LibraryInfoId",
                table: "Libraries",
                column: "LibraryInfoId",
                principalTable: "LibraryInfo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Libraries_LibraryInfo_LibraryInfoId",
                table: "Libraries");

            migrationBuilder.DropTable(
                name: "LibraryInfo");

            migrationBuilder.DropIndex(
                name: "IX_Libraries_LibraryInfoId",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "LibraryInfoId",
                table: "Libraries");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Libraries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
