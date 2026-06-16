using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Versions;
using FlowSpace.Domain.Repositories;

namespace FlowSpace.Application.Versions.Queries.GetVersionDetails;

public record GetVersionDetailsQuery(Guid VersionId) : IQuery<VersionDetailResponse>;

public class GetVersionDetailsQueryHandler : IQueryHandler<GetVersionDetailsQuery, VersionDetailResponse>
{
    private readonly IBoardVersionRepository _versionRepository;

    public GetVersionDetailsQueryHandler(IBoardVersionRepository versionRepository)
    {
        _versionRepository = versionRepository;
    }

    public async Task<Result<VersionDetailResponse>> Handle(GetVersionDetailsQuery query, CancellationToken cancellationToken)
    {
        var version = await _versionRepository.GetWithSnapshotAsync(query.VersionId, cancellationToken);

        if (version is null)
        {
            return Result.Failure<VersionDetailResponse>(new Error("Version.NotFound", "Version not found."));
        }

        return new VersionDetailResponse(
            version.Id,
            version.Snapshot.NodesData,
            version.Snapshot.EdgesData,
            version.Snapshot.YjsState);
    }
}
