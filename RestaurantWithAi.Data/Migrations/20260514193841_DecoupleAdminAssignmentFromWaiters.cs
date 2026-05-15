using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantWithAi.Data.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleAdminAssignmentFromWaiters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminAssignments_Waiters_AppointedById",
                table: "AdminAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AdminAssignments_Waiters_AppointedUserId",
                table: "AdminAssignments");

            migrationBuilder.AddColumn<Guid>(
                name: "RestaurantId",
                table: "AdminAssignments",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "AdminAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminAssignments_Waiters_AppointedById",
                table: "AdminAssignments",
                column: "AppointedById",
                principalTable: "Waiters",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminAssignments_Waiters_AppointedUserId",
                table: "AdminAssignments",
                column: "AppointedUserId",
                principalTable: "Waiters",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
