using Kreisverkehr.Extensions.Hosting.BatchWorker.Options;
using Microsoft.Extensions.Options;

namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Internal.Queues;

internal sealed class FixedIntervalQueue<TWorkItem> : IBatchQueue<TWorkItem>
{
    private readonly Queue<TWorkItem> _queue = new();
    private readonly IOptions<FixedTimeQueueOptions<TWorkItem>> _options;
    private readonly Timer _timer;

    public FixedIntervalQueue(IOptions<FixedTimeQueueOptions<TWorkItem>> options)
    {
        _options = options;
        _timer = new(SetBatchReady, null, TimeSpan.Zero, _options.Value.Interval);
    }

    public bool BatchReady { get; private set; }

    private void SetBatchReady(object? stateInfo)
    {
        BatchReady = _queue.Any();
    }

    public IEnumerable<TWorkItem> DequeueNextBatch()
    {
        BatchReady = false;
        while (_queue.Any())
            yield return _queue.Dequeue();
    }

    public void Enqueue(TWorkItem item) =>
        _queue.Enqueue(item);
}