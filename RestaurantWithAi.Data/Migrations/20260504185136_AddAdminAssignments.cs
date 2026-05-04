using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantWithAi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointedById = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AppointedUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminAssignments_Waiters_AppointedById",
                        column: x => x.AppointedById,
                        principalTable: "Waiters",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminAssignments_Waiters_AppointedUserId",
                        column: x => x.AppointedUserId,
                        principalTable: "Waiters",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAssignments_AppointedById",
                table: "AdminAssignments",
                column: "AppointedById");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAssignments_AppointedUserId",
                table: "AdminAssignments",
                column: "AppointedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminAssignments");
        }
    }
}
