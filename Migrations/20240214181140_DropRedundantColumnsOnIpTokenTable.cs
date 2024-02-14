using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smug.Migrations
{
    /// <inheritdoc />
    public partial class DropRedundantColumnsOnIpTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpId",
                table: "IpToken");
            
            migrationBuilder.DropColumn(
                name: "TokenId",
                table: "IpToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IpId",
                table: "IpToken",
                type: "uuid",
                nullable: false);
            
            migrationBuilder.AddColumn<Guid>(
                name: "TokenId",
                table: "IpToken",
                type: "uuid",
                nullable: false);
        }
    }
}
