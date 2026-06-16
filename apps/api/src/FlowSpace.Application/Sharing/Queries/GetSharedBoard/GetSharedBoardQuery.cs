using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Sharing.Queries.GetSharedBoard;

public record GetSharedBoardQuery(string Token) : IQuery<BoardResponse>;

public class GetSharedBoardQueryHandler : IQueryHandler<GetSharedBoardQuery, BoardResponse>
{
    private readonly IBoardShareLinkRepository _shareLinkRepository;

    public GetSharedBoardQueryHandler(IBoardShareLinkRepository shareLinkRepository)
    {
        _shareLinkRepository = shareLinkRepository;
    }

    public async Task<Result<BoardResponse>> Handle(GetSharedBoardQuery query, CancellationToken cancellationToken)
    {
        var link = await _shareLinkRepository.GetByTokenAsync(query.Token, cancellationToken);
        
        if (link is null || !link.IsValid())
        {
            return Result.Failure<BoardResponse>(new Error("ShareLink.Invalid", "The share link is invalid or has expired."));
        }

        var board = link.Board;
        return new BoardResponse(
            board.Id,
            board.WorkspaceId,
            board.Name,
            board.Type,
            board.CreatedAt
        );
    }
}
