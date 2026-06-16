using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Interop;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Interop.Converters;
using System.Text.Json;
using System.Text;

namespace FlowSpace.Application.Interop.Commands.ExportBoard;

public record ExportBoardCommand(Guid BoardId, string Format) : ICommand<ExportResponse>;

public class ExportBoardCommandHandler : ICommandHandler<ExportBoardCommand, ExportResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly INodeRepository _nodeRepository;
    private readonly IEdgeRepository _edgeRepository;

    public ExportBoardCommandHandler(
        IBoardRepository boardRepository,
        INodeRepository nodeRepository,
        IEdgeRepository edgeRepository)
    {
        _boardRepository = boardRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
    }

    public async Task<Result<ExportResponse>> Handle(ExportBoardCommand command, CancellationToken cancellationToken)
    {
        var board = await _boardRepository.GetByIdAsync(command.BoardId, cancellationToken);
        if (board is null) return Result.Failure<ExportResponse>(new Error("Board.NotFound", "Board not found."));

        var nodes = await _nodeRepository.GetByBoardIdAsync(command.BoardId, cancellationToken);
        var edges = await _edgeRepository.GetByBoardIdAsync(command.BoardId, cancellationToken);

        byte[] data;
        string contentType;
        string fileName = $"{board.Name.Replace(" ", "_")}";

        switch (command.Format.ToLower())
        {
            case "flowspace":
                var native = FlowSpaceConverter.ToNative(board, nodes, edges);
                data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(native));
                contentType = "application/json";
                fileName += ".flowspace";
                break;
            case "drawio":
                var xml = DrawIoConverter.ToDrawIoXml(board, nodes, edges);
                data = Encoding.UTF8.GetBytes(xml);
                contentType = "application/xml";
                fileName += ".drawio";
                break;
            case "svg":
                // Placeholder for headless rendering
                data = Encoding.UTF8.GetBytes("<svg>Placeholder</svg>");
                contentType = "image/svg+xml";
                fileName += ".svg";
                break;
            default:
                return Result.Failure<ExportResponse>(new Error("Interop.UnsupportedFormat", "Unsupported export format."));
        }

        return new ExportResponse(fileName, contentType, data);
    }
}
