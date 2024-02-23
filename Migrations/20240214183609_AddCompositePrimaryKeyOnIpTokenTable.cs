using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kyoto.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositePrimaryKeyOnIpTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_IpToken",
                table: "IpToken",
                columns: new[] { "IpAddressInfoId", "TokenInfoId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IpToken",
                table: "IpToken");
        }
    }
}
