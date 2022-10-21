namespace Kreisverkehr.Extensions.Hosting.BatchWorker;

public interface IBatchQueue<TWorkItem>
{
    void Enqueue(TWorkItem item);

    bool BatchReady { get; }

    IEnumerable<TWorkItem> DequeueNextBatch();
}