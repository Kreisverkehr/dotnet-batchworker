namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Options;

public sealed class MaxSizeMaxIntervalQueueOptions<TWorkItem>
{
    public TimeSpan Interval { get; set; }
    public int Size { get; set; }
}