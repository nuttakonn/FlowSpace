using FlowSpace.Domain.Common;
using FlowSpace.Domain.Authorization;

namespace FlowSpace.Domain.Entities;

public class BoardShareLink : Entity
{
    public Guid BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    public string Token { get; private set; } = string.Empty;
    public BoardRole Role { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private BoardShareLink() { }

    public static BoardShareLink Create(Guid boardId, BoardRole role, DateTime? expiresAt = null)
    {
        return new BoardShareLink
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            Token = GenerateToken(),
            Role = role,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsValid()
    {
        if (IsRevoked) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow) return false;
        return true;
    }

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "")
            .Substring(0, 32);
    }
}
