using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using FlowSpace.Application.Common.Abstractions.Interop;

namespace FlowSpace.Infrastructure.Interop;

public class CanvasExportWorker : BackgroundService
{
    private readonly IExportQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CanvasExportWorker> _logger;

    public CanvasExportWorker(
        IExportQueue queue, 
        IServiceProvider serviceProvider, 
        ILogger<CanvasExportWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Canvas Export Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _queue.DequeueAsync(stoppingToken);
                _logger.LogInformation("Processing export job {JobId} for Board {BoardId} (Format: {Format})", 
                    job.JobId, job.BoardId, job.Format);

                using var scope = _serviceProvider.CreateScope();
                var exportService = scope.ServiceProvider.GetRequiredService<IExportService>();

                byte[] data;
                switch (job.Format.ToLower())
                {
                    case "png":
                        data = await exportService.ExportToPngAsync(job.BoardId, stoppingToken);
                        break;
                    case "pdf":
                        data = await exportService.ExportToPdfAsync(job.BoardId, stoppingToken);
                        break;
                    default:
                        _logger.LogWarning("Unsupported format {Format} in background job.", job.Format);
                        continue;
                }

                // Future: Upload 'data' to MinIO/S3 and mark job as 'Completed' in a JobTracking table
                _logger.LogInformation("Successfully completed export job {JobId}. Size: {Size} bytes", job.JobId, data.Length);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing export job.");
            }
        }

        _logger.LogInformation("Canvas Export Worker is stopping.");
    }
}
