namespace Kreisverkehr.Extensions.Hosting.BatchWorker;

public interface IBatchProcessor<TWorkItem>
{
    Task<BatchResult> ProcessBatchAsync(IEnumerable<TWorkItem> batch);
}