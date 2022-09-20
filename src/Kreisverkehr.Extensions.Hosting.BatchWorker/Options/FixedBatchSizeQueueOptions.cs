namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Options;

public sealed class FixedBatchSizeQueueOptions<TWorkItem>
{
    public int Size { get; set; } = 15;
}