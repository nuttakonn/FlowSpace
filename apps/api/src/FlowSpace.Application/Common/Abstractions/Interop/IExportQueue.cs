using System.Threading.Channels;

namespace FlowSpace.Application.Common.Abstractions.Interop;

public interface IExportQueue
{
    ValueTask EnqueueAsync(ExportJob job, CancellationToken cancellationToken = default);
    ValueTask<ExportJob> DequeueAsync(CancellationToken cancellationToken = default);
}

public record ExportJob(Guid JobId, Guid BoardId, string Format);

public class ExportQueue : IExportQueue
{
    private readonly Channel<ExportJob> _queue;

    public ExportQueue()
    {
        // Unbounded for simplicity, in prod use Bounded with Drop/Wait strategy
        _queue = Channel.CreateUnbounded<ExportJob>();
    }

    public ValueTask EnqueueAsync(ExportJob job, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(job, cancellationToken);
    }

    public ValueTask<ExportJob> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
