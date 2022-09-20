using Kreisverkehr.Extensions.Hosting.BatchWorker.Options;
using Microsoft.Extensions.Options;

namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Internal.Queues;

public sealed class MaxSizeMaxIntervalQueue<TWorkItem> : IBatchQueue<TWorkItem>
{
    private readonly Queue<TWorkItem> _queue = new();
    private readonly IOptions<MaxSizeMaxIntervalQueueOptions<TWorkItem>> _options;
    private readonly Timer _timer;

    public MaxSizeMaxIntervalQueue(IOptions<MaxSizeMaxIntervalQueueOptions<TWorkItem>> options)
    {
        _options = options;
        _timer = new(SetBatchReady, null, TimeSpan.Zero, _options.Value.Interval);
    }

    public bool BatchReady { get; private set; }
    
    private void SetBatchReady(object? stateInfo)
    {
        BatchReady = _queue.Any();
    }

    private void ResetQueueState()
    {
        BatchReady = false;
        _timer.Change(TimeSpan.Zero, _options.Value.Interval);
    }

    public IEnumerable<TWorkItem> DequeueNextBatch()
    {
        ResetQueueState();
        int itemsDequeued = 0;
        while (itemsDequeued++ < _options.Value.Size && _queue.Any())
            yield return _queue.Dequeue();
    }

    public void Enqueue(TWorkItem item)
    {
        _queue.Enqueue(item);
        BatchReady = _queue.Count >= _options.Value.Size;
    }
}