using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RBACModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "role",
                table: "workspace_members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "board_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_permissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_permissions_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_board_permissions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_board_permissions_board_id_user_id",
                table: "board_permissions",
                columns: new[] { "board_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_board_permissions_user_id",
                table: "board_permissions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board_permissions");

            migrationBuilder.DropColumn(
                name: "role",
                table: "workspace_members");
        }
    }
}
