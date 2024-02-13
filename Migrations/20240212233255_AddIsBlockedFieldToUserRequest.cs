using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smug.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBlockedFieldToUserRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "UserRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "UserRequests");
        }
    }
}
