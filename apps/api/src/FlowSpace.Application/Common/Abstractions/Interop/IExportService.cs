namespace FlowSpace.Application.Common.Abstractions.Interop;

public interface IExportService
{
    Task<byte[]> ExportToPngAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportToPdfAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<string> ExportToSvgAsync(Guid boardId, CancellationToken cancellationToken = default);
}
