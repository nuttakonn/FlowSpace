using FlowSpace.Domain.Common;

namespace FlowSpace.Domain.Entities;

public class RefreshToken : Entity
{
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(string tokenHash, DateTime expiresAt, Guid userId)
    {
        return new RefreshToken
        {
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            UserId = userId
        };
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
