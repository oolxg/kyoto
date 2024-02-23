using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kyoto.Migrations
{
    /// <inheritdoc />
    public partial class DropIdColumnOnIpTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "IpToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "IpToken",
                type: "integer",
                nullable: false);
        }
    }
}
