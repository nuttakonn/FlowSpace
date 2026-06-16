using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Boards.Queries.GetBoardById;

public record GetBoardByIdQuery(Guid BoardId) : IQuery<BoardResponse>;

public class GetBoardByIdQueryHandler : IQueryHandler<GetBoardByIdQuery, BoardResponse>
{
    private readonly IBoardRepository _boardRepository;

    public GetBoardByIdQueryHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<BoardResponse>> Handle(GetBoardByIdQuery query, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, cancellationToken);
        if (board is null)
        {
            return Result.Failure<BoardResponse>(new Error("Board.NotFound", "The board was not found."));
        }

        return new BoardResponse(
            board.Id,
            board.WorkspaceId,
            board.Name,
            board.Type,
            board.CreatedAt
        );
    }
}
