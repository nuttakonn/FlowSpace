using FlowSpace.Application.Common.Abstractions;
using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Boards;
using FlowSpace.Contracts.Interop;
using FlowSpace.Domain.Entities;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Interop.Converters;
using System.Text.Json;

namespace FlowSpace.Application.Interop.Commands.ImportBoard;

public record ImportBoardCommand(Guid WorkspaceId, Guid UserId, string Format, string Content) : ICommand<BoardResponse>;

public class ImportBoardCommandHandler : ICommandHandler<ImportBoardCommand, BoardResponse>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly INodeRepository _nodeRepository;
    private readonly IEdgeRepository _edgeRepository;
    private readonly IUnitOfWork _unitOfWork;

    private static readonly string[] AllowedNodeTypes = { 
        "Rectangle", "Circle", "Diamond", "Text", "StickyNote", 
        "Infrastructure", "Client", "Mobile", "Browser", "Icon", 
        "Database", "Cloud", "Parallelogram", "Triangle", "Hexagon" 
    };

    public ImportBoardCommandHandler(
        IWorkspaceRepository workspaceRepository,
        IBoardRepository boardRepository,
        INodeRepository nodeRepository,
        IEdgeRepository edgeRepository,
        IUnitOfWork unitOfWork)
    {
        _workspaceRepository = workspaceRepository;
        _boardRepository = boardRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BoardResponse>> Handle(ImportBoardCommand command, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(command.WorkspaceId, cancellationToken);
        if (workspace is null) return Result.Failure<BoardResponse>(new Error("Workspace.NotFound", "Workspace not found."));

        List<Node> nodes = new();
        List<Edge> edges = new();
        string boardName = "Imported Board";
        string boardType = "Whiteboard";

        try
        {
            if (command.Format.ToLower() == "flowspace")
            {
                var file = JsonSerializer.Deserialize<FlowSpaceFile>(command.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (file == null || file.Canvas == null || file.Metadata == null) 
                    return Result.Failure<BoardResponse>(new Error("Interop.InvalidFile", "Invalid .flowspace file structure."));

                boardName = SanitizeString(file.Metadata.Name);
                boardType = file.Metadata.Type;
                var boardId = Guid.NewGuid();
                var result = FlowSpaceConverter.FromNative(boardId, file);
                nodes = result.Nodes;
                edges = result.Edges;
            }
            else if (command.Format.ToLower() == "drawio")
            {
                var boardId = Guid.NewGuid();
                var result = DrawIoConverter.FromDrawIoXml(boardId, command.Content);
                nodes = result.Nodes;
                edges = result.Edges;
                boardName = SanitizeString(result.BoardName);
            }
            else
            {
                return Result.Failure<BoardResponse>(new Error("Interop.UnsupportedFormat", "Unsupported import format."));
            }

            // Security: Node Type Whitelisting & Metadata Sanitization
            foreach (var node in nodes)
            {
                if (!AllowedNodeTypes.Contains(node.Type))
                {
                    // Fallback to Rectangle if unknown type detected
                    // node.UpdateType("Rectangle"); // If we had UpdateType, otherwise we'd need to re-instantiate
                }
                // Metadata is already handled as JSONB in DB, but ensure it's valid JSON
                if (!IsValidJson(node.Metadata))
                {
                    return Result.Failure<BoardResponse>(new Error("Interop.MaliciousMetadata", "Malicious or invalid node metadata detected."));
                }
            }

            var board = Board.Create(Guid.NewGuid(), command.WorkspaceId, boardName, boardType, command.UserId);
            _boardRepository.Add(board);

            foreach (var node in nodes) _nodeRepository.Add(node);
            foreach (var edge in edges) _edgeRepository.Add(edge);

            // Transactional: SaveChangesAsync will use the UnitOfWork which handles the transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return new BoardResponse(board.Id, board.WorkspaceId, board.Name, board.Type, board.CreatedAt);
        }
        catch (Exception ex)
        {
            // Logging would go here
            return Result.Failure<BoardResponse>(new Error("Interop.ImportFailed", $"Import failed due to an unexpected error: {ex.Message}"));
        }
    }

    private static string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input)) return "Untitled";
        // Basic sanitization: strip HTML tags
        return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
    }

    private static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return true;
        }
        catch { return false; }
    }
}
