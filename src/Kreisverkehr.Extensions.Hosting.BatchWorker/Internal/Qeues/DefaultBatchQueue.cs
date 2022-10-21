namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Internal.Queues;

internal sealed class DefaultBatchQueue<TWorkItem> : IBatchQueue<TWorkItem>
{
    private readonly Queue<TWorkItem> _queue = new();
    public bool BatchReady => _queue.Any();

    public IEnumerable<TWorkItem> DequeueNextBatch()
    {
        TWorkItem[] batch = new TWorkItem[1];
        batch[0] = _queue.Dequeue();
        return batch;
    }

    public void Enqueue(TWorkItem item) =>
        _queue.Enqueue(item);
}