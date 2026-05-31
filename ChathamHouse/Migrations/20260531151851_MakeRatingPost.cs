using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChathamHouse.Migrations
{
    /// <inheritdoc />
    public partial class MakeRatingPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Posts_PostId",
                table: "Ratings");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Ratings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Posts_PostId",
                table: "Ratings",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Posts_PostId",
                table: "Ratings");

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Ratings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Posts_PostId",
                table: "Ratings",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
