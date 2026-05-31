using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChathamHouse.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveStream : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StreamTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StreamKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StreamUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsLive = table.Column<bool>(type: "bit", nullable: false),
                    ViewerCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalLikes = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreams_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StreamComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StreamComments_LiveStreams_StreamId",
                        column: x => x.StreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StreamLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StreamLikes_LiveStreams_StreamId",
                        column: x => x.StreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StreamViewers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamViewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamViewers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StreamViewers_LiveStreams_StreamId",
                        column: x => x.StreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreams_StreamKey",
                table: "LiveStreams",
                column: "StreamKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreams_UserId",
                table: "LiveStreams",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamComments_StreamId",
                table: "StreamComments",
                column: "StreamId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamComments_UserId",
                table: "StreamComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamLikes_StreamId_UserId",
                table: "StreamLikes",
                columns: new[] { "StreamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamLikes_UserId",
                table: "StreamLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamViewers_StreamId_UserId",
                table: "StreamViewers",
                columns: new[] { "StreamId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StreamViewers_UserId",
                table: "StreamViewers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreamComments");

            migrationBuilder.DropTable(
                name: "StreamLikes");

            migrationBuilder.DropTable(
                name: "StreamViewers");

            migrationBuilder.DropTable(
                name: "LiveStreams");
        }
    }
}
