using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Boards;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Boards.Queries.ListBoards;

public record ListBoardsQuery(Guid WorkspaceId) : IQuery<List<BoardResponse>>;

public class ListBoardsQueryHandler : IQueryHandler<ListBoardsQuery, List<BoardResponse>>
{
    private readonly IBoardRepository _boardRepository;

    public ListBoardsQueryHandler(IBoardRepository boardRepository)
    {
        _boardRepository = boardRepository;
    }

    public async Task<Result<List<BoardResponse>>> Handle(ListBoardsQuery query, CancellationToken cancellationToken)
    {
        var boards = await _boardRepository.ListByWorkspaceIdAsync(query.WorkspaceId, cancellationToken);

        var response = boards.Select(b => new BoardResponse(b.Id, b.WorkspaceId, b.Name, b.Type, b.CreatedAt)).ToList();

        return response;
    }
}
