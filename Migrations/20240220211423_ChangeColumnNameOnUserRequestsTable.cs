using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smug.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnNameOnUserRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BlockReason",
                table: "UserRequests",
                newName: "DecisionReason");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DecisionReason",
                table: "UserRequests",
                newName: "BlockReason");
        }
    }
}
