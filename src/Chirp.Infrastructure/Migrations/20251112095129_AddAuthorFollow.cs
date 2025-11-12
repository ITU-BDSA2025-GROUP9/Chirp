using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chirp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorFollow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorAuthor",
                columns: table => new
                {
                    FollowersId = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowingId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorAuthor", x => new { x.FollowersId, x.FollowingId });
                    table.ForeignKey(
                        name: "FK_AuthorAuthor_AspNetUsers_FollowersId",
                        column: x => x.FollowersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorAuthor_AspNetUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorAuthor_FollowingId",
                table: "AuthorAuthor",
                column: "FollowingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorAuthor");
        }
    }
}
