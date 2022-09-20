using Kreisverkehr.Extensions.Hosting.BatchWorker.Options;
using Kreisverkehr.Extensions.Hosting.BatchWorker.Internal.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Internal;

internal sealed class DefaultBatchQueueFactory<TWorkItem> : IBatchQueueFactory<TWorkItem>
{
    private readonly IOptions<BatchProcessorOptions<TWorkItem>> _options;
    private readonly IServiceProvider _serviceProvider;

    public DefaultBatchQueueFactory(IOptions<BatchProcessorOptions<TWorkItem>> options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public IBatchQueue<TWorkItem> CreateQueue()
    {
        var queueType = _options.Value.Type switch 
        {
            QueueType.FixedBatchSize     => typeof(FixedBatchSizeQueue<TWorkItem>),
            QueueType.FixedInterval      => typeof(FixedIntervalQueue<TWorkItem>),
            QueueType.MaxSizeMaxInterval => typeof(MaxSizeMaxIntervalQueue<TWorkItem>),
            _                            => typeof(DefaultBatchQueue<TWorkItem>)
        };

        var queueInstance = ActivatorUtilities.CreateInstance(_serviceProvider, queueType);
        return (IBatchQueue<TWorkItem>)queueInstance;
    }
}