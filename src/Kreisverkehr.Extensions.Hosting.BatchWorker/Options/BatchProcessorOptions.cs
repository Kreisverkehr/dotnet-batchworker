namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Options;

public sealed class BatchProcessorOptions<TWorkItem>
{
    public QueueType Type { get; set; } = QueueType.FixedBatchSize;
    public int PollWaitTime { get; set; }
    public int MaxIncreases { get; set; }
}

public enum QueueType 
{
    FixedBatchSize,
    FixedInterval,
    MaxSizeMaxInterval
}