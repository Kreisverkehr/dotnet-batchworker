using Kreisverkehr.Extensions.Hosting.BatchWorker.Options;
using Microsoft.Extensions.Options;

namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Internal.Queues;

internal sealed class FixedBatchSizeQueue<TWorkItem> : IBatchQueue<TWorkItem>
{
    private readonly Queue<TWorkItem> _queue = new();
    private readonly IOptions<FixedBatchSizeQueueOptions<TWorkItem>> _options;

    public FixedBatchSizeQueue(IOptions<FixedBatchSizeQueueOptions<TWorkItem>> options)
    {
        _options = options;
    }

    public bool BatchReady => _queue.Count >= _options.Value.Size;

    public IEnumerable<TWorkItem> DequeueNextBatch()
    {
        TWorkItem[] batch = new TWorkItem[_options.Value.Size];
        for(int i = 0; i < _options.Value.Size; i++)
            batch[i] = _queue.Dequeue();

        return batch;
    }

    public void Enqueue(TWorkItem item) =>
        _queue.Enqueue(item);
}

public sealed class FixedBatchSizeQueueOptions 
{
    public int Size { get; set; } = 5;
}