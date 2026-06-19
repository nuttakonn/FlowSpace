namespace FlowSpace.Application.Common.Abstractions.Interop;

public interface IExportService
{
    Task<byte[]> ExportToPngAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", CancellationToken cancellationToken = default);
    Task<byte[]> ExportToJpgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", CancellationToken cancellationToken = default);
    Task<byte[]> ExportToPdfAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", CancellationToken cancellationToken = default);
    Task<string> ExportToSvgAsync(Guid boardId, string jwtToken = "", string frontendBaseUrl = "", CancellationToken cancellationToken = default);
}
