using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Versions;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Versions.Queries.GetVersions;

public record GetVersionsQuery(Guid BoardId) : IQuery<List<BoardVersionResponse>>;

public class GetVersionsQueryHandler : IQueryHandler<GetVersionsQuery, List<BoardVersionResponse>>
{
    private readonly IBoardVersionRepository _versionRepository;

    public GetVersionsQueryHandler(IBoardVersionRepository versionRepository)
    {
        _versionRepository = versionRepository;
    }

    public async Task<Result<List<BoardVersionResponse>>> Handle(GetVersionsQuery query, CancellationToken cancellationToken)
    {
        var versions = await _versionRepository.GetByBoardIdAsync(query.BoardId, cancellationToken);

        var response = versions.Select(v => new BoardVersionResponse(
            v.Id,
            v.BoardId,
            v.Name,
            v.Description,
            v.CreatedAt,
            v.CreatedBy,
            v.Creator?.DisplayName ?? "Unknown",
            (int)v.Type)).ToList();

        return response;
    }
}
