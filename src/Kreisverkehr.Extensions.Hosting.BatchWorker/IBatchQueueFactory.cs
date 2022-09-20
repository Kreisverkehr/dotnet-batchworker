namespace Kreisverkehr.Extensions.Hosting.BatchWorker;

public interface IBatchQueueFactory<TWorkItem>
{
    IBatchQueue<TWorkItem> CreateQueue();
}