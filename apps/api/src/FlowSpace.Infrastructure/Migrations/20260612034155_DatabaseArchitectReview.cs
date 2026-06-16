using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseArchitectReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boards_workspaces_WorkspaceId",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "FK_edges_boards_BoardId",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "FK_edges_nodes_SourceNodeId",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "FK_edges_nodes_TargetNodeId",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "FK_nodes_boards_BoardId",
                table: "nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_workspace_members_users_UserId",
                table: "workspace_members");

            migrationBuilder.DropForeignKey(
                name: "FK_workspace_members_workspaces_WorkspaceId",
                table: "workspace_members");

            migrationBuilder.DropForeignKey(
                name: "FK_workspaces_users_OwnerId",
                table: "workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workspaces",
                table: "workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_workspace_members",
                table: "workspace_members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_nodes",
                table: "nodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_edges",
                table: "edges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_boards",
                table: "boards");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "workspaces",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "workspaces",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "workspaces",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "workspaces",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "workspaces",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "workspaces",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "workspaces",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_workspaces_OwnerId",
                table: "workspaces",
                newName: "ix_workspaces_owner_id");

            migrationBuilder.RenameColumn(
                name: "JoinedAt",
                table: "workspace_members",
                newName: "joined_at");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "workspace_members",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "workspace_members",
                newName: "workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_workspace_members_UserId",
                table: "workspace_members",
                newName: "ix_workspace_members_user_id");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "users",
                newName: "display_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "users",
                newName: "avatar_url");

            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameColumn(
                name: "Y",
                table: "nodes",
                newName: "y");

            migrationBuilder.RenameColumn(
                name: "X",
                table: "nodes",
                newName: "x");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "nodes",
                newName: "width");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "nodes",
                newName: "version");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "nodes",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "nodes",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "Height",
                table: "nodes",
                newName: "height");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "nodes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "BoardId",
                table: "nodes",
                newName: "board_id");

            migrationBuilder.RenameIndex(
                name: "IX_nodes_BoardId",
                table: "nodes",
                newName: "ix_nodes_board_id");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "edges",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "edges",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TargetNodeId",
                table: "edges",
                newName: "target_node_id");

            migrationBuilder.RenameColumn(
                name: "SourceNodeId",
                table: "edges",
                newName: "source_node_id");

            migrationBuilder.RenameColumn(
                name: "BoardId",
                table: "edges",
                newName: "board_id");

            migrationBuilder.RenameIndex(
                name: "IX_edges_TargetNodeId",
                table: "edges",
                newName: "ix_edges_target_node_id");

            migrationBuilder.RenameIndex(
                name: "IX_edges_SourceNodeId",
                table: "edges",
                newName: "ix_edges_source_node_id");

            migrationBuilder.RenameIndex(
                name: "IX_edges_BoardId",
                table: "edges",
                newName: "ix_edges_board_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "boards",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "boards",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "boards",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WorkspaceId",
                table: "boards",
                newName: "workspace_id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "boards",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "boards",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "boards",
                newName: "deleted_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "boards",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_boards_WorkspaceId",
                table: "boards",
                newName: "ix_boards_workspace_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_workspaces",
                table: "workspaces",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_workspace_members",
                table: "workspace_members",
                columns: new[] { "workspace_id", "user_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_nodes",
                table: "nodes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_edges",
                table: "edges",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_boards",
                table: "boards",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_is_deleted",
                table: "workspaces",
                column: "is_deleted",
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_nodes_metadata",
                table: "nodes",
                column: "metadata")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_edges_metadata",
                table: "edges",
                column: "metadata")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_boards_is_deleted",
                table: "boards",
                column: "is_deleted",
                filter: "is_deleted = false");

            migrationBuilder.AddForeignKey(
                name: "fk_boards_workspaces_workspace_id",
                table: "boards",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_edges_boards_board_id",
                table: "edges",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_edges_nodes_source_node_id",
                table: "edges",
                column: "source_node_id",
                principalTable: "nodes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_edges_nodes_target_node_id",
                table: "edges",
                column: "target_node_id",
                principalTable: "nodes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_nodes_boards_board_id",
                table: "nodes",
                column: "board_id",
                principalTable: "boards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_workspace_members_users_user_id",
                table: "workspace_members",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_workspace_members_workspaces_workspace_id",
                table: "workspace_members",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_workspaces_users_owner_id",
                table: "workspaces",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_boards_workspaces_workspace_id",
                table: "boards");

            migrationBuilder.DropForeignKey(
                name: "fk_edges_boards_board_id",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "fk_edges_nodes_source_node_id",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "fk_edges_nodes_target_node_id",
                table: "edges");

            migrationBuilder.DropForeignKey(
                name: "fk_nodes_boards_board_id",
                table: "nodes");

            migrationBuilder.DropForeignKey(
                name: "fk_workspace_members_users_user_id",
                table: "workspace_members");

            migrationBuilder.DropForeignKey(
                name: "fk_workspace_members_workspaces_workspace_id",
                table: "workspace_members");

            migrationBuilder.DropForeignKey(
                name: "fk_workspaces_users_owner_id",
                table: "workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "pk_workspaces",
                table: "workspaces");

            migrationBuilder.DropIndex(
                name: "ix_workspaces_is_deleted",
                table: "workspaces");

            migrationBuilder.DropPrimaryKey(
                name: "pk_workspace_members",
                table: "workspace_members");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_nodes",
                table: "nodes");

            migrationBuilder.DropIndex(
                name: "ix_nodes_metadata",
                table: "nodes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_edges",
                table: "edges");

            migrationBuilder.DropIndex(
                name: "ix_edges_metadata",
                table: "edges");

            migrationBuilder.DropPrimaryKey(
                name: "pk_boards",
                table: "boards");

            migrationBuilder.DropIndex(
                name: "ix_boards_is_deleted",
                table: "boards");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "workspaces",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "workspaces",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "workspaces",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "workspaces",
                newName: "OwnerId");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "workspaces",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "workspaces",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "workspaces",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_workspaces_owner_id",
                table: "workspaces",
                newName: "IX_workspaces_OwnerId");

            migrationBuilder.RenameColumn(
                name: "joined_at",
                table: "workspace_members",
                newName: "JoinedAt");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "workspace_members",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "workspace_id",
                table: "workspace_members",
                newName: "WorkspaceId");

            migrationBuilder.RenameIndex(
                name: "ix_workspace_members_user_id",
                table: "workspace_members",
                newName: "IX_workspace_members_UserId");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "users",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "avatar_url",
                table: "users",
                newName: "AvatarUrl");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "users",
                newName: "IX_users_Email");

            migrationBuilder.RenameColumn(
                name: "y",
                table: "nodes",
                newName: "Y");

            migrationBuilder.RenameColumn(
                name: "x",
                table: "nodes",
                newName: "X");

            migrationBuilder.RenameColumn(
                name: "width",
                table: "nodes",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "version",
                table: "nodes",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "nodes",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "nodes",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "height",
                table: "nodes",
                newName: "Height");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "nodes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "board_id",
                table: "nodes",
                newName: "BoardId");

            migrationBuilder.RenameIndex(
                name: "ix_nodes_board_id",
                table: "nodes",
                newName: "IX_nodes_BoardId");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "edges",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "edges",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "target_node_id",
                table: "edges",
                newName: "TargetNodeId");

            migrationBuilder.RenameColumn(
                name: "source_node_id",
                table: "edges",
                newName: "SourceNodeId");

            migrationBuilder.RenameColumn(
                name: "board_id",
                table: "edges",
                newName: "BoardId");

            migrationBuilder.RenameIndex(
                name: "ix_edges_target_node_id",
                table: "edges",
                newName: "IX_edges_TargetNodeId");

            migrationBuilder.RenameIndex(
                name: "ix_edges_source_node_id",
                table: "edges",
                newName: "IX_edges_SourceNodeId");

            migrationBuilder.RenameIndex(
                name: "ix_edges_board_id",
                table: "edges",
                newName: "IX_edges_BoardId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "boards",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "boards",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "boards",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "workspace_id",
                table: "boards",
                newName: "WorkspaceId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "boards",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "boards",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                table: "boards",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "boards",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_boards_workspace_id",
                table: "boards",
                newName: "IX_boards_WorkspaceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workspaces",
                table: "workspaces",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_workspace_members",
                table: "workspace_members",
                columns: new[] { "WorkspaceId", "UserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_nodes",
                table: "nodes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_edges",
                table: "edges",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_boards",
                table: "boards",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_boards_workspaces_WorkspaceId",
                table: "boards",
                column: "WorkspaceId",
                principalTable: "workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_edges_boards_BoardId",
                table: "edges",
                column: "BoardId",
                principalTable: "boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_edges_nodes_SourceNodeId",
                table: "edges",
                column: "SourceNodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_edges_nodes_TargetNodeId",
                table: "edges",
                column: "TargetNodeId",
                principalTable: "nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_nodes_boards_BoardId",
                table: "nodes",
                column: "BoardId",
                principalTable: "boards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workspace_members_users_UserId",
                table: "workspace_members",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workspace_members_workspaces_WorkspaceId",
                table: "workspace_members",
                column: "WorkspaceId",
                principalTable: "workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workspaces_users_OwnerId",
                table: "workspaces",
                column: "OwnerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
