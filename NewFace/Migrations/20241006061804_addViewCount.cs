using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewFace.Migrations
{
    /// <inheritdoc />
    public partial class addViewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ViewCount",
                table: "ActorDemoStars",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "ActorDemoStars");
        }
    }
}
