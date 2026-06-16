using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.AI;

namespace FlowSpace.Application.Common.Abstractions.AI;

public interface IAiService
{
    Task<Result<AiDiagramResponse>> GenerateDiagramAsync(AiDiagramRequest request, CancellationToken cancellationToken = default);
}
