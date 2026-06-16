using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Diagrams;
using FlowSpace.Domain.Repositories;
using System.Text.Json;

namespace FlowSpace.Application.Boards.Queries.GetWhiteboard;

public record GetWhiteboardQuery(Guid BoardId) : IQuery<WhiteboardResponse>;

public class GetWhiteboardQueryHandler : IQueryHandler<GetWhiteboardQuery, WhiteboardResponse>
{
    private readonly INodeRepository _nodeRepository;

    public GetWhiteboardQueryHandler(INodeRepository nodeRepository)
    {
        _nodeRepository = nodeRepository;
    }

    public async Task<Result<WhiteboardResponse>> Handle(GetWhiteboardQuery query, CancellationToken cancellationToken)
    {
        var nodes = await _nodeRepository.GetByBoardIdAsync(query.BoardId, cancellationToken);
        
        var records = new Dictionary<string, object>();
        foreach (var node in nodes.Where(n => n.Type == "WhiteboardRecord"))
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(node.Metadata);
            if (data != null && data.TryGetValue("id", out var recordId))
            {
                records[recordId.ToString()!] = data;
            }
        }

        return new WhiteboardResponse(records);
    }
}
