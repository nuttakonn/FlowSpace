using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BoardSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "visibility",
                table: "boards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "board_share_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_share_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_share_links_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: false),
                    board_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    content_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_templates", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_templates_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_board_share_links_board_id",
                table: "board_share_links",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_share_links_token",
                table: "board_share_links",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_board_templates_created_by",
                table: "board_templates",
                column: "created_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board_share_links");

            migrationBuilder.DropTable(
                name: "board_templates");

            migrationBuilder.DropColumn(
                name: "visibility",
                table: "boards");
        }
    }
}
