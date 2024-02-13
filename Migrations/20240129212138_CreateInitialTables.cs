using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smug.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IpAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ip = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusChangeReason = table.Column<string>(type: "text", nullable: true),
                    StatusChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShouldHideIfBanned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RestrictedUrls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Host = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    RestrictedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestrictedUrls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IpToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IpId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    IpAddressInfoId = table.Column<Guid>(type: "uuid", nullable: true),
                    TokenInfoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IpToken_IpAddresses_IpAddressInfoId",
                        column: x => x.IpAddressInfoId,
                        principalTable: "IpAddresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IpToken_Tokens_TokenInfoId",
                        column: x => x.TokenInfoId,
                        principalTable: "Tokens",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenInfoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Host = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    Headers = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRequests_IpAddresses_IpInfoId",
                        column: x => x.IpInfoId,
                        principalTable: "IpAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRequests_Tokens_TokenInfoId",
                        column: x => x.TokenInfoId,
                        principalTable: "Tokens",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IpToken_IpAddressInfoId",
                table: "IpToken",
                column: "IpAddressInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_IpToken_TokenInfoId",
                table: "IpToken",
                column: "TokenInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequests_IpInfoId",
                table: "UserRequests",
                column: "IpInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequests_TokenInfoId",
                table: "UserRequests",
                column: "TokenInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IpToken");

            migrationBuilder.DropTable(
                name: "RestrictedUrls");

            migrationBuilder.DropTable(
                name: "UserRequests");

            migrationBuilder.DropTable(
                name: "IpAddresses");

            migrationBuilder.DropTable(
                name: "Tokens");
        }
    }
}
