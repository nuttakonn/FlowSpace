using FlowSpace.Application.Common.Abstractions.Messaging;
using FlowSpace.Application.Common.Models;
using FlowSpace.Contracts.Interop;
using FlowSpace.Domain.Repositories;
using FlowSpace.Application.Interop.Converters;
using FlowSpace.Application.Common.Abstractions.Interop;
using System.Text.Json;
using System.Text;

namespace FlowSpace.Application.Interop.Commands.ExportBoard;

public record ExportBoardCommand(Guid BoardId, string Format, string JwtToken = "", string FrontendBaseUrl = "") : ICommand<ExportResponse>;

public class ExportBoardCommandHandler : ICommandHandler<ExportBoardCommand, ExportResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly INodeRepository _nodeRepository;
    private readonly IEdgeRepository _edgeRepository;
    private readonly IExportService _exportService;

    public ExportBoardCommandHandler(
        IBoardRepository boardRepository,
        INodeRepository nodeRepository,
        IEdgeRepository edgeRepository,
        IExportService exportService)
    {
        _boardRepository = boardRepository;
        _nodeRepository = nodeRepository;
        _edgeRepository = edgeRepository;
        _exportService = exportService;
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
            case "png":
                data = await _exportService.ExportToPngAsync(command.BoardId, command.JwtToken, command.FrontendBaseUrl, cancellationToken);
                contentType = "image/png";
                fileName += ".png";
                break;
            case "jpg":
            case "jpeg":
                data = await _exportService.ExportToJpgAsync(command.BoardId, command.JwtToken, command.FrontendBaseUrl, cancellationToken);
                contentType = "image/jpeg";
                fileName += ".jpg";
                break;
            case "pdf":
                data = await _exportService.ExportToPdfAsync(command.BoardId, command.JwtToken, command.FrontendBaseUrl, cancellationToken);
                contentType = "application/pdf";
                fileName += ".pdf";
                break;
            case "svg":
                var svgContent = await _exportService.ExportToSvgAsync(command.BoardId, command.JwtToken, command.FrontendBaseUrl, cancellationToken);
                data = Encoding.UTF8.GetBytes(svgContent);
                contentType = "image/svg+xml";
                fileName += ".svg";
                break;
            default:
                return Result.Failure<ExportResponse>(new Error("Interop.UnsupportedFormat", "Unsupported export format."));
        }

        return new ExportResponse(fileName, contentType, data);
    }
}
