using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantWithAi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "OrderItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderItems");
        }
    }
}
