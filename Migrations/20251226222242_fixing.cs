using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtGallery.Migrations
{
    /// <inheritdoc />
    public partial class fixing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinnerUserId",
                table: "Artist",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artist_WinnerUserId",
                table: "Artist",
                column: "WinnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Artist_Users_WinnerUserId",
                table: "Artist",
                column: "WinnerUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artist_Users_WinnerUserId",
                table: "Artist");

            migrationBuilder.DropIndex(
                name: "IX_Artist_WinnerUserId",
                table: "Artist");

            migrationBuilder.DropColumn(
                name: "WinnerUserId",
                table: "Artist");
        }
    }
}
