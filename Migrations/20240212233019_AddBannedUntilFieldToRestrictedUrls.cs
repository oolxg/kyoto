using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kyoto.Migrations
{
    /// <inheritdoc />
    public partial class AddBannedUntilFieldToRestrictedUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BannedUntil",
                table: "RestrictedUrls",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannedUntil",
                table: "RestrictedUrls");
        }
    }
}
