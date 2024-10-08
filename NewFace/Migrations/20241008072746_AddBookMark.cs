using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFace.Migrations
{
    /// <inheritdoc />
    public partial class AddBookMark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LikesFromActors",
                table: "Actors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikesFromCommons",
                table: "Actors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikesFromEnters",
                table: "Actors",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikesFromActors",
                table: "Actors");

            migrationBuilder.DropColumn(
                name: "LikesFromCommons",
                table: "Actors");

            migrationBuilder.DropColumn(
                name: "LikesFromEnters",
                table: "Actors");
        }
    }
}
