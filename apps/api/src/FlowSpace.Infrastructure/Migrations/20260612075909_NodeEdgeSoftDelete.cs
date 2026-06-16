using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NodeEdgeSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "nodes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "nodes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "edges",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "edges",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_nodes_is_deleted",
                table: "nodes",
                column: "is_deleted",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_edges_is_deleted",
                table: "edges",
                column: "is_deleted",
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_nodes_is_deleted",
                table: "nodes");

            migrationBuilder.DropIndex(
                name: "ix_edges_is_deleted",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "nodes");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "nodes");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "edges");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "edges");
        }
    }
}
