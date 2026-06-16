using FlowSpace.Domain.Common;
using FlowSpace.Domain.Authorization;

namespace FlowSpace.Domain.Entities;

public class BoardPermission : Entity
{
    public Guid BoardId { get; private set; }
    public Board Board { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public BoardRole Role { get; private set; }

    private BoardPermission() { }

    public static BoardPermission Create(Guid boardId, Guid userId, BoardRole role)
    {
        return new BoardPermission
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            UserId = userId,
            Role = role
        };
    }

    public void UpdateRole(BoardRole role)
    {
        Role = role;
    }
}
