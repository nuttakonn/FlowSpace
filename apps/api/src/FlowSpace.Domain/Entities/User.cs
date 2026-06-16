using FlowSpace.Domain.Common;

namespace FlowSpace.Domain.Entities;

public class User : AggregateRoot, IAuditableEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<Workspace> _ownedWorkspaces = new();
    public IReadOnlyCollection<Workspace> OwnedWorkspaces => _ownedWorkspaces.AsReadOnly();

    private readonly List<WorkspaceMember> _memberships = new();
    public IReadOnlyCollection<WorkspaceMember> Memberships => _memberships.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { } // EF Core

    public static User Create(Guid id, string email, string passwordHash, string displayName, string? avatarUrl = null)
    {
        return new User
        {
            Id = id,
            Email = email,
            PasswordHash = passwordHash,
            DisplayName = displayName,
            AvatarUrl = avatarUrl
        };
    }

    public void AddRefreshToken(string tokenHash, DateTime expiresAt)
    {
        _refreshTokens.Add(RefreshToken.Create(tokenHash, expiresAt, Id));
    }

    public void RevokeRefreshToken(string tokenHash)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);
        if (refreshToken != null)
        {
            refreshToken.Revoke();
        }
    }

    public bool HasValidRefreshToken(string tokenHash)
    {
        return _refreshTokens.Any(rt => rt.TokenHash == tokenHash && rt.IsActive);
    }
}
