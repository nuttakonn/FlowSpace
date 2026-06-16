using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CanvasVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "canvas_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nodes_data = table.Column<string>(type: "jsonb", nullable: false),
                    edges_data = table.Column<string>(type: "jsonb", nullable: false),
                    yjs_state = table.Column<byte[]>(type: "bytea", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canvas_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_canvas_snapshots_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_versions_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_board_versions_canvas_snapshots_snapshot_id",
                        column: x => x.snapshot_id,
                        principalTable: "canvas_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_board_versions_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_board_versions_board_id",
                table: "board_versions",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_versions_created_by",
                table: "board_versions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_board_versions_snapshot_id",
                table: "board_versions",
                column: "snapshot_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_canvas_snapshots_board_id",
                table: "canvas_snapshots",
                column: "board_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "board_versions");

            migrationBuilder.DropTable(
                name: "canvas_snapshots");
        }
    }
}
