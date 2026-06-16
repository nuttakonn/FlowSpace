using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilteredCompoundIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspaces_is_deleted",
                table: "workspaces");

            migrationBuilder.DropIndex(
                name: "ix_workspaces_owner_id",
                table: "workspaces");

            migrationBuilder.DropIndex(
                name: "ix_nodes_board_id",
                table: "nodes");

            migrationBuilder.DropIndex(
                name: "ix_nodes_is_deleted",
                table: "nodes");

            migrationBuilder.DropIndex(
                name: "ix_edges_board_id",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "ix_edges_is_deleted",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "ix_boards_is_deleted",
                table: "boards");

            migrationBuilder.DropIndex(
                name: "ix_boards_workspace_id",
                table: "boards");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_owner_id",
                table: "workspaces",
                column: "owner_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_nodes_board_id",
                table: "nodes",
                column: "board_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_edges_board_id",
                table: "edges",
                column: "board_id",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_boards_workspace_id",
                table: "boards",
                column: "workspace_id",
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspaces_owner_id",
                table: "workspaces");

            migrationBuilder.DropIndex(
                name: "ix_nodes_board_id",
                table: "nodes");

            migrationBuilder.DropIndex(
                name: "ix_edges_board_id",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "ix_boards_workspace_id",
                table: "boards");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_is_deleted",
                table: "workspaces",
                column: "is_deleted",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_owner_id",
                table: "workspaces",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_nodes_board_id",
                table: "nodes",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_nodes_is_deleted",
                table: "nodes",
                column: "is_deleted",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_edges_board_id",
                table: "edges",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_edges_is_deleted",
                table: "edges",
                column: "is_deleted",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_boards_is_deleted",
                table: "boards",
                column: "is_deleted",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_boards_workspace_id",
                table: "boards",
                column: "workspace_id");
        }
    }
}
