using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowSpace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HashRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "token",
                table: "refresh_tokens",
                newName: "token_hash");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token_hash");

            migrationBuilder.AddColumn<DateTime>(
                name: "revoked_at",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "refresh_tokens");

            migrationBuilder.RenameColumn(
                name: "token_hash",
                table: "refresh_tokens",
                newName: "token");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token");
        }
    }
}
