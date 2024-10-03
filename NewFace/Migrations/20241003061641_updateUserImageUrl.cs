using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFace.Migrations
{
    /// <inheritdoc />
    public partial class updateUserImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Users",
                newName: "PublicUrl");

            migrationBuilder.AddColumn<string>(
                name: "StoragePath",
                table: "Users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoragePath",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PublicUrl",
                table: "Users",
                newName: "ImageUrl");
        }
    }
}
