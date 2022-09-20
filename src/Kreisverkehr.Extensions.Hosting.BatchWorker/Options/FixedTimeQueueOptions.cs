namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Options;

public sealed class FixedTimeQueueOptions<TWorkItem>
{
    public TimeSpan Interval { get; set; }
}