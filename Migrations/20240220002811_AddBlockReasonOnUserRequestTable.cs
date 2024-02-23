using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kyoto.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockReasonOnUserRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockReason",
                table: "UserRequests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockReason",
                table: "UserRequests");
        }
    }
}
