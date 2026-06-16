using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Sharing;
using FlowSpace.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace FlowSpace.Application.Sharing.Queries.GetBoardSharingInfo;

public record GetBoardSharingInfoQuery(Guid BoardId) : IQuery<BoardSharingInfoResponse>;

public class GetBoardSharingInfoQueryHandler : IQueryHandler<GetBoardSharingInfoQuery, BoardSharingInfoResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IBoardShareLinkRepository _shareLinkRepository;
    private readonly string _frontendUrl;

    public GetBoardSharingInfoQueryHandler(
        IBoardRepository boardRepository,
        IBoardShareLinkRepository shareLinkRepository,
        IConfiguration configuration)
    {
        _boardRepository = boardRepository;
        _shareLinkRepository = shareLinkRepository;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
    }

    public async Task<Result<BoardSharingInfoResponse>> Handle(GetBoardSharingInfoQuery query, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(query.BoardId, cancellationToken);
        if (board is null) return Result.Failure<BoardSharingInfoResponse>(new Error("Board.NotFound", "Board not found."));

        var shareLinks = await _shareLinkRepository.GetByBoardIdAsync(query.BoardId, cancellationToken);
        
        var shareLinkResponses = shareLinks.Select(sl => new ShareLinkResponse(
            sl.Id,
            sl.Token,
            (int)sl.Role,
            sl.ExpiresAt,
            sl.IsRevoked,
            sl.CreatedAt,
            $"{_frontendUrl}/shared/{sl.Token}"
        )).ToList();

        var permissionResponses = board.Permissions.Select(p => new BoardPermissionResponse(
            p.UserId,
            p.User?.Email ?? "Unknown",
            p.User?.DisplayName ?? "Unknown",
            (int)p.Role
        )).ToList();

        return new BoardSharingInfoResponse(
            board.Id,
            (int)board.Visibility,
            permissionResponses,
            shareLinkResponses
        );
    }
}
