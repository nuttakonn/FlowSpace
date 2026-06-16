using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using System.Text.Json;

namespace FlowSpace.Application.Versions.Commands.RestoreVersion;

public record RestoreVersionCommand(Guid VersionId) : ICommand;

public class RestoreVersionCommandHandler : ICommandHandler<RestoreVersionCommand>
{
    private readonly IBoardVersionRepository _versionRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly INodeRepository _nodeRepository;
    private readonly IEdgeRepository _edgeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RestoreVersionCommandHandler(
        IBoardVersionRepository versionRepository,
        IBoardRepository boardRepository,
        INodeRepository nodeRepository,
        IEdgeRepository edgeRepository,
        IUnitOfWork unitOfWork)
    {
        _versionRepository = versionRepository;
        _boardRepository = boardRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RestoreVersionCommand command, CancellationToken cancellationToken)
    {
        var version = await _versionRepository.GetWithSnapshotAsync(command.VersionId, cancellationToken);

        if (version is null)
        {
            return Result.Failure(new Error("Version.NotFound", "Version not found."));
        }

        var boardId = version.BoardId;
        var board = await _boardRepository.GetByIdAsync(boardId, cancellationToken);
        if (board == null) return Result.Failure(new Error("Board.NotFound", "Board not found."));

        // 1. Overwrite elements
        var currentNodes = await _nodeRepository.GetByBoardIdAsync(boardId, cancellationToken);
        var currentEdges = await _edgeRepository.GetByBoardIdAsync(boardId, cancellationToken);

        foreach (var node in currentNodes) _nodeRepository.Delete(node);
        foreach (var edge in currentEdges) _edgeRepository.Delete(edge);

        if (board.Type == "Whiteboard")
        {
            // Rebuild logic for Whiteboard nodes if needed for culling.
            // For now, the binary YjsState in the snapshot is the source of truth.
        }
        else
        {
            // Parse and restore nodes from snapshot
            var nodesData = JsonSerializer.Deserialize<List<NodeSnapshot>>(version.Snapshot.NodesData);
            if (nodesData != null)
            {
                foreach (var nodeDto in nodesData)
                {
                    var node = Node.Create(nodeDto.Id, boardId, nodeDto.Type, nodeDto.X, nodeDto.Y);
                    node.Update(nodeDto.X, nodeDto.Y, nodeDto.Width, nodeDto.Height);
                    node.SetMetadata(nodeDto.Metadata);
                    _nodeRepository.Add(node);
                }
            }

            // Parse and restore edges
            var edgesData = JsonSerializer.Deserialize<List<EdgeSnapshot>>(version.Snapshot.EdgesData);
            if (edgesData != null)
            {
                foreach (var edgeDto in edgesData)
                {
                    var edge = Edge.Create(edgeDto.Id, boardId, edgeDto.SourceNodeId, edgeDto.TargetNodeId);
                    edge.SetMetadata(edgeDto.Metadata);
                    _edgeRepository.Add(edge);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private record NodeSnapshot(Guid Id, string Type, double X, double Y, double? Width, double? Height, string Metadata);
    private record EdgeSnapshot(Guid Id, Guid SourceNodeId, Guid TargetNodeId, string Metadata);
}
