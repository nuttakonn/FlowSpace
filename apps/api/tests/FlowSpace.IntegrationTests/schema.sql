CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE users (
    "Id" uuid NOT NULL,
    "Email" character varying(255) NOT NULL,
    "DisplayName" character varying(100) NOT NULL,
    "AvatarUrl" character varying(2048),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_users" PRIMARY KEY ("Id")
);

CREATE TABLE workspaces (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "OwnerId" uuid NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_workspaces" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_workspaces_users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES users ("Id") ON DELETE RESTRICT
);

CREATE TABLE boards (
    "Id" uuid NOT NULL,
    "WorkspaceId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Type" character varying(50) NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_boards" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_boards_workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES workspaces ("Id") ON DELETE CASCADE
);

CREATE TABLE workspace_members (
    "WorkspaceId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "JoinedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_workspace_members" PRIMARY KEY ("WorkspaceId", "UserId"),
    CONSTRAINT "FK_workspace_members_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_workspace_members_workspaces_WorkspaceId" FOREIGN KEY ("WorkspaceId") REFERENCES workspaces ("Id") ON DELETE CASCADE
);

CREATE TABLE nodes (
    "Id" uuid NOT NULL,
    "BoardId" uuid NOT NULL,
    "Type" character varying(50) NOT NULL,
    "X" double precision NOT NULL,
    "Y" double precision NOT NULL,
    "Width" double precision,
    "Height" double precision,
    "Metadata" jsonb NOT NULL,
    "Version" integer NOT NULL,
    CONSTRAINT "PK_nodes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_nodes_boards_BoardId" FOREIGN KEY ("BoardId") REFERENCES boards ("Id") ON DELETE CASCADE
);

CREATE TABLE edges (
    "Id" uuid NOT NULL,
    "BoardId" uuid NOT NULL,
    "SourceNodeId" uuid NOT NULL,
    "TargetNodeId" uuid NOT NULL,
    "Metadata" jsonb NOT NULL,
    CONSTRAINT "PK_edges" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_edges_boards_BoardId" FOREIGN KEY ("BoardId") REFERENCES boards ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_edges_nodes_SourceNodeId" FOREIGN KEY ("SourceNodeId") REFERENCES nodes ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_edges_nodes_TargetNodeId" FOREIGN KEY ("TargetNodeId") REFERENCES nodes ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_boards_WorkspaceId" ON boards ("WorkspaceId");

CREATE INDEX "IX_edges_BoardId" ON edges ("BoardId");

CREATE INDEX "IX_edges_SourceNodeId" ON edges ("SourceNodeId");

CREATE INDEX "IX_edges_TargetNodeId" ON edges ("TargetNodeId");

CREATE INDEX "IX_nodes_BoardId" ON nodes ("BoardId");

CREATE UNIQUE INDEX "IX_users_Email" ON users ("Email");

CREATE INDEX "IX_workspace_members_UserId" ON workspace_members ("UserId");

CREATE INDEX "IX_workspaces_OwnerId" ON workspaces ("OwnerId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260612034023_InitialCreate', '10.0.9');

COMMIT;

START TRANSACTION;
ALTER TABLE boards DROP CONSTRAINT "FK_boards_workspaces_WorkspaceId";

ALTER TABLE edges DROP CONSTRAINT "FK_edges_boards_BoardId";

ALTER TABLE edges DROP CONSTRAINT "FK_edges_nodes_SourceNodeId";

ALTER TABLE edges DROP CONSTRAINT "FK_edges_nodes_TargetNodeId";

ALTER TABLE nodes DROP CONSTRAINT "FK_nodes_boards_BoardId";

ALTER TABLE workspace_members DROP CONSTRAINT "FK_workspace_members_users_UserId";

ALTER TABLE workspace_members DROP CONSTRAINT "FK_workspace_members_workspaces_WorkspaceId";

ALTER TABLE workspaces DROP CONSTRAINT "FK_workspaces_users_OwnerId";

ALTER TABLE workspaces DROP CONSTRAINT "PK_workspaces";

ALTER TABLE workspace_members DROP CONSTRAINT "PK_workspace_members";

ALTER TABLE users DROP CONSTRAINT "PK_users";

ALTER TABLE nodes DROP CONSTRAINT "PK_nodes";

ALTER TABLE edges DROP CONSTRAINT "PK_edges";

ALTER TABLE boards DROP CONSTRAINT "PK_boards";

ALTER TABLE workspaces RENAME COLUMN "Name" TO name;

ALTER TABLE workspaces RENAME COLUMN "Id" TO id;

ALTER TABLE workspaces RENAME COLUMN "UpdatedAt" TO updated_at;

ALTER TABLE workspaces RENAME COLUMN "OwnerId" TO owner_id;

ALTER TABLE workspaces RENAME COLUMN "IsDeleted" TO is_deleted;

ALTER TABLE workspaces RENAME COLUMN "DeletedAt" TO deleted_at;

ALTER TABLE workspaces RENAME COLUMN "CreatedAt" TO created_at;

ALTER INDEX "IX_workspaces_OwnerId" RENAME TO ix_workspaces_owner_id;

ALTER TABLE workspace_members RENAME COLUMN "JoinedAt" TO joined_at;

ALTER TABLE workspace_members RENAME COLUMN "UserId" TO user_id;

ALTER TABLE workspace_members RENAME COLUMN "WorkspaceId" TO workspace_id;

ALTER INDEX "IX_workspace_members_UserId" RENAME TO ix_workspace_members_user_id;

ALTER TABLE users RENAME COLUMN "Email" TO email;

ALTER TABLE users RENAME COLUMN "Id" TO id;

ALTER TABLE users RENAME COLUMN "UpdatedAt" TO updated_at;

ALTER TABLE users RENAME COLUMN "DisplayName" TO display_name;

ALTER TABLE users RENAME COLUMN "CreatedAt" TO created_at;

ALTER TABLE users RENAME COLUMN "AvatarUrl" TO avatar_url;

ALTER INDEX "IX_users_Email" RENAME TO ix_users_email;

ALTER TABLE nodes RENAME COLUMN "Y" TO y;

ALTER TABLE nodes RENAME COLUMN "X" TO x;

ALTER TABLE nodes RENAME COLUMN "Width" TO width;

ALTER TABLE nodes RENAME COLUMN "Version" TO version;

ALTER TABLE nodes RENAME COLUMN "Type" TO type;

ALTER TABLE nodes RENAME COLUMN "Metadata" TO metadata;

ALTER TABLE nodes RENAME COLUMN "Height" TO height;

ALTER TABLE nodes RENAME COLUMN "Id" TO id;

ALTER TABLE nodes RENAME COLUMN "BoardId" TO board_id;

ALTER INDEX "IX_nodes_BoardId" RENAME TO ix_nodes_board_id;

ALTER TABLE edges RENAME COLUMN "Metadata" TO metadata;

ALTER TABLE edges RENAME COLUMN "Id" TO id;

ALTER TABLE edges RENAME COLUMN "TargetNodeId" TO target_node_id;

ALTER TABLE edges RENAME COLUMN "SourceNodeId" TO source_node_id;

ALTER TABLE edges RENAME COLUMN "BoardId" TO board_id;

ALTER INDEX "IX_edges_TargetNodeId" RENAME TO ix_edges_target_node_id;

ALTER INDEX "IX_edges_SourceNodeId" RENAME TO ix_edges_source_node_id;

ALTER INDEX "IX_edges_BoardId" RENAME TO ix_edges_board_id;

ALTER TABLE boards RENAME COLUMN "Type" TO type;

ALTER TABLE boards RENAME COLUMN "Name" TO name;

ALTER TABLE boards RENAME COLUMN "Id" TO id;

ALTER TABLE boards RENAME COLUMN "WorkspaceId" TO workspace_id;

ALTER TABLE boards RENAME COLUMN "UpdatedAt" TO updated_at;

ALTER TABLE boards RENAME COLUMN "IsDeleted" TO is_deleted;

ALTER TABLE boards RENAME COLUMN "DeletedAt" TO deleted_at;

ALTER TABLE boards RENAME COLUMN "CreatedAt" TO created_at;

ALTER INDEX "IX_boards_WorkspaceId" RENAME TO ix_boards_workspace_id;

ALTER TABLE workspaces ADD CONSTRAINT pk_workspaces PRIMARY KEY (id);

ALTER TABLE workspace_members ADD CONSTRAINT pk_workspace_members PRIMARY KEY (workspace_id, user_id);

ALTER TABLE users ADD CONSTRAINT pk_users PRIMARY KEY (id);

ALTER TABLE nodes ADD CONSTRAINT pk_nodes PRIMARY KEY (id);

ALTER TABLE edges ADD CONSTRAINT pk_edges PRIMARY KEY (id);

ALTER TABLE boards ADD CONSTRAINT pk_boards PRIMARY KEY (id);

CREATE INDEX ix_workspaces_is_deleted ON workspaces (is_deleted) WHERE is_deleted = false;

CREATE INDEX ix_nodes_metadata ON nodes USING gin (metadata);

CREATE INDEX ix_edges_metadata ON edges USING gin (metadata);

CREATE INDEX ix_boards_is_deleted ON boards (is_deleted) WHERE is_deleted = false;

ALTER TABLE boards ADD CONSTRAINT fk_boards_workspaces_workspace_id FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE;

ALTER TABLE edges ADD CONSTRAINT fk_edges_boards_board_id FOREIGN KEY (board_id) REFERENCES boards (id) ON DELETE CASCADE;

ALTER TABLE edges ADD CONSTRAINT fk_edges_nodes_source_node_id FOREIGN KEY (source_node_id) REFERENCES nodes (id) ON DELETE RESTRICT;

ALTER TABLE edges ADD CONSTRAINT fk_edges_nodes_target_node_id FOREIGN KEY (target_node_id) REFERENCES nodes (id) ON DELETE RESTRICT;

ALTER TABLE nodes ADD CONSTRAINT fk_nodes_boards_board_id FOREIGN KEY (board_id) REFERENCES boards (id) ON DELETE CASCADE;

ALTER TABLE workspace_members ADD CONSTRAINT fk_workspace_members_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE;

ALTER TABLE workspace_members ADD CONSTRAINT fk_workspace_members_workspaces_workspace_id FOREIGN KEY (workspace_id) REFERENCES workspaces (id) ON DELETE CASCADE;

ALTER TABLE workspaces ADD CONSTRAINT fk_workspaces_users_owner_id FOREIGN KEY (owner_id) REFERENCES users (id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260612034155_DatabaseArchitectReview', '10.0.9');

COMMIT;

START TRANSACTION;
ALTER TABLE users ADD password_hash text NOT NULL DEFAULT '';

CREATE TABLE refresh_tokens (
    id uuid NOT NULL,
    token character varying(500) NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    user_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    CONSTRAINT pk_refresh_tokens PRIMARY KEY (id),
    CONSTRAINT fk_refresh_tokens_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_refresh_tokens_token ON refresh_tokens (token);

CREATE INDEX ix_refresh_tokens_user_id ON refresh_tokens (user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260612034435_AuthenticationModule', '10.0.9');

COMMIT;

START TRANSACTION;
ALTER TABLE workspace_members ADD role integer NOT NULL DEFAULT 0;

CREATE TABLE board_permissions (
    id uuid NOT NULL,
    board_id uuid NOT NULL,
    user_id uuid NOT NULL,
    role integer NOT NULL,
    CONSTRAINT pk_board_permissions PRIMARY KEY (id),
    CONSTRAINT fk_board_permissions_boards_board_id FOREIGN KEY (board_id) REFERENCES boards (id) ON DELETE CASCADE,
    CONSTRAINT fk_board_permissions_users_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_board_permissions_board_id_user_id ON board_permissions (board_id, user_id);

CREATE INDEX ix_board_permissions_user_id ON board_permissions (user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260612034915_RBACModule', '10.0.9');

COMMIT;

START TRANSACTION;
ALTER TABLE refresh_tokens RENAME COLUMN token TO token_hash;

ALTER INDEX ix_refresh_tokens_token RENAME TO ix_refresh_tokens_token_hash;

ALTER TABLE refresh_tokens ADD revoked_at timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260612075603_HashRefreshTokens', '10.0.9');

COMMIT;

START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260612075804_SoftDeleteConsistency', '10.0.9');

COMMIT;

START TRANSACTION;
ALTER TABLE nodes ADD deleted_at timestamp with time zone;

ALTER TABLE nodes ADD is_deleted boolean NOT NULL DEFAULT FALSE;

ALTER TABLE edges ADD deleted_at timestamp with time zone;

ALTER TABLE edges ADD is_deleted boolean NOT NULL DEFAULT FALSE;

CREATE INDEX ix_nodes_is_deleted ON nodes (is_deleted) WHERE is_deleted = false;

CREATE INDEX ix_edges_is_deleted ON edges (is_deleted) WHERE is_deleted = false;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260612075909_NodeEdgeSoftDelete', '10.0.9');

COMMIT;

