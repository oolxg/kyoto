﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kyoto.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHiddenFieldOnUserRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "UserRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "UserRequests");
        }
    }
}
