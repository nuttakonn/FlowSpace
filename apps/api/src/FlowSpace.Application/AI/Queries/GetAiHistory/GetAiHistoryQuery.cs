using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.AI;
using FlowSpace.Domain.Repositories;
using System.Text.Json;

namespace FlowSpace.Application.AI.Queries.GetAiHistory;

public record GetAiHistoryQuery(Guid BoardId) : IQuery<List<AiGenerationHistoryResponse>>;

public class GetAiHistoryQueryHandler : IQueryHandler<GetAiHistoryQuery, List<AiGenerationHistoryResponse>>
{
    private readonly IAiGenerationHistoryRepository _historyRepository;

    public GetAiHistoryQueryHandler(IAiGenerationHistoryRepository historyRepository)
    {
        _historyRepository = historyRepository;
    }

    public async Task<Result<List<AiGenerationHistoryResponse>>> Handle(GetAiHistoryQuery query, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByBoardIdAsync(query.BoardId, cancellationToken);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var response = history.Select(h => new AiGenerationHistoryResponse(
            h.Id,
            h.BoardId,
            h.Prompt,
            h.DiagramType,
            JsonSerializer.Deserialize<AiDiagramResponse>(h.ResultJson, options)!,
            h.CreatedAt,
            h.User?.DisplayName ?? "Unknown")).ToList();

        return response;
    }
}
