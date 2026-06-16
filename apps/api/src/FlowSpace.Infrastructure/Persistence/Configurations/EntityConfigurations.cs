using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FlowSpace.Domain.Entities;

namespace FlowSpace.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(2048);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();
    }
}

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(w => w.Owner)
            .WithMany(u => u.OwnedWorkspaces)
            .HasForeignKey(w => w.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => w.OwnerId)
               .HasFilter("is_deleted = false");

        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}

public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
{
    public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
    {
        builder.ToTable("workspace_members");

        builder.HasKey(wm => new { wm.WorkspaceId, wm.UserId });

        builder.HasOne(wm => wm.Workspace)
            .WithMany(w => w.Members)
            .HasForeignKey(wm => wm.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wm => wm.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(wm => wm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(wm => wm.UserId);

        builder.Property(wm => wm.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.HasQueryFilter(wm => !wm.Workspace.IsDeleted);
    }
}

public class BoardPermissionConfiguration : IEntityTypeConfiguration<BoardPermission>
{
    public void Configure(EntityTypeBuilder<BoardPermission> builder)
    {
        builder.ToTable("board_permissions");

        builder.HasKey(bp => bp.Id);

        builder.HasOne(bp => bp.Board)
            .WithMany(b => b.Permissions)
            .HasForeignKey(bp => bp.BoardId);

        builder.HasOne(bp => bp.User)
            .WithMany()
            .HasForeignKey(bp => bp.UserId);

        builder.Property(bp => bp.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(bp => new { bp.BoardId, bp.UserId }).IsUnique();

        builder.HasQueryFilter(bp => !bp.Board.IsDeleted);
    }
}

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.ToTable("boards");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(b => b.Visibility)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(b => b.Workspace)
            .WithMany(w => w.Boards)
            .HasForeignKey(b => b.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.ShareLinks)
            .WithOne(sl => sl.Board)
            .HasForeignKey(sl => sl.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<BoardVersion>()
            .WithOne(bv => bv.Board)
            .HasForeignKey(bv => bv.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.WorkspaceId)
               .HasFilter("is_deleted = false");

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}

public class BoardShareLinkConfiguration : IEntityTypeConfiguration<BoardShareLink>
{
    public void Configure(EntityTypeBuilder<BoardShareLink> builder)
    {
        builder.ToTable("board_share_links");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Token)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sl => sl.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(sl => sl.Board)
            .WithMany(b => b.ShareLinks)
            .HasForeignKey(sl => sl.BoardId);

        builder.HasIndex(sl => sl.Token)
            .IsUnique();

        builder.HasIndex(sl => sl.BoardId);
    }
}

public class BoardVersionConfiguration : IEntityTypeConfiguration<BoardVersion>
{
    public void Configure(EntityTypeBuilder<BoardVersion> builder)
    {
        builder.ToTable("board_versions");

        builder.HasKey(bv => bv.Id);

        builder.Property(bv => bv.Name)
            .HasMaxLength(200);

        builder.Property(bv => bv.Description)
            .HasMaxLength(1000);

        builder.Property(bv => bv.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(bv => bv.Creator)
            .WithMany()
            .HasForeignKey(bv => bv.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bv => bv.Snapshot)
            .WithOne()
            .HasForeignKey<BoardVersion>(bv => bv.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bv => bv.BoardId);
    }
}

public class CanvasSnapshotConfiguration : IEntityTypeConfiguration<CanvasSnapshot>
{
    public void Configure(EntityTypeBuilder<CanvasSnapshot> builder)
    {
        builder.ToTable("canvas_snapshots");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.NodesData)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(cs => cs.EdgesData)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(cs => cs.YjsState)
            .HasColumnType("bytea")
            .IsRequired();

        builder.HasOne(cs => cs.Board)
            .WithMany()
            .HasForeignKey(cs => cs.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AiGenerationRequestConfiguration : IEntityTypeConfiguration<AiGenerationRequest>
{
    public void Configure(EntityTypeBuilder<AiGenerationRequest> builder)
    {
        builder.ToTable("ai_generation_requests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Prompt)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.DiagramType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ResultJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(x => x.Board)
            .WithMany()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BoardId);
    }
}

public class BoardTemplateConfiguration : IEntityTypeConfiguration<BoardTemplate>
{
    public void Configure(EntityTypeBuilder<BoardTemplate> builder)
    {
        builder.ToTable("board_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.BoardType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ContentJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasOne(x => x.Creator)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class NodeConfiguration : IEntityTypeConfiguration<Node>
{
    public void Configure(EntityTypeBuilder<Node> builder)
    {
        builder.ToTable("nodes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Metadata)
            .HasColumnType("jsonb");

        builder.Property(n => n.Version)
            .IsConcurrencyToken();

        builder.HasOne(n => n.Board)
            .WithMany(b => b.Nodes)
            .HasForeignKey(n => n.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.BoardId)
               .HasFilter("is_deleted = false");
               
        builder.HasIndex(n => n.Metadata).HasMethod("gin");

        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}

public class EdgeConfiguration : IEntityTypeConfiguration<Edge>
{
    public void Configure(EntityTypeBuilder<Edge> builder)
    {
        builder.ToTable("edges");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Metadata)
            .HasColumnType("jsonb");

        builder.HasOne(e => e.Board)
            .WithMany(b => b.Edges)
            .HasForeignKey(e => e.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SourceNode)
            .WithMany()
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetNode)
            .WithMany()
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.BoardId)
               .HasFilter("is_deleted = false");
               
        builder.HasIndex(e => e.Metadata).HasMethod("gin");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
