using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chirp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
            
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers
                SET ProfileImage = '/images/bird' || (abs(random()) % 5 + 1) || '-profile.png'
                WHERE ProfileImage IS NULL OR ProfileImage = '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "AspNetUsers");
        }
    }
}
