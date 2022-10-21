using Kreisverkehr.Extensions.Hosting.BatchWorker.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kreisverkehr.Extensions.Hosting.BatchWorker.Internal;

internal sealed class DefaultBatchWorker<TWorkItem> : BackgroundService
{
    private readonly ILogger<DefaultBatchWorker<TWorkItem>> _logger;
    private readonly IBatchQueue<TWorkItem> _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<BatchProcessorOptions<TWorkItem>> _options;

    private int _waitTime = 0;
    private int _waitTimeIncreasesLeft = 0;

    public DefaultBatchWorker(
        ILogger<DefaultBatchWorker<TWorkItem>> logger, 
        IBatchQueue<TWorkItem> queue, 
        IOptions<BatchProcessorOptions<TWorkItem>> options, 
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _queue = queue;
        _serviceProvider = serviceProvider;
        _options = options;
        ResetWaitTime();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_waitTime, stoppingToken);
            if (!_queue.BatchReady)
                continue;

            _logger.LogDebug("Batch ready.");
            var batch = _queue.DequeueNextBatch().ToArray();
            _logger.LogDebug("Batch retrieved.");

            var result = await ProcessBatchAsync(batch);

            if (result == BatchResult.Success)
            {
                _logger.LogInformation("Batch sucessfully processed.");
                ResetWaitTime();
            }

            if (result == BatchResult.TemporaryError)
            {
                _logger.LogWarning("Batch failed. Error may be gone next time.");
                _logger.LogDebug("Requeueing items.");
                foreach (var item in batch)
                    _queue.Enqueue(item);
                IncreaseWaitTime();
            }

            if(result == BatchResult.Error)
            {
                _logger.LogWarning("Batch failed. Trying to reprocess individual items.");
                await ReprocessFailedBatch(batch);
            }
        }
    }

    private async Task<IEnumerable<TWorkItem>> ReprocessFailedBatch(IEnumerable<TWorkItem> batch)
    {
        if(batch.Count() == 1)
        {
            var result = await ProcessBatchAsync(batch);
            if(result == BatchResult.Success)
                return Enumerable.Empty<TWorkItem>();
            if(result == BatchResult.Error)
                return batch;
            if(result == BatchResult.TemporaryError)
            {
                _queue.Enqueue(batch.First());
                return Enumerable.Empty<TWorkItem>();
            }
        }

        int chunkSize = batch.Count() / 2;
        List<TWorkItem> corruptItems = new();

        foreach(var batchChunk in batch.Chunk(chunkSize))
        {
            var result = await ProcessBatchAsync(batchChunk);
            if(result == BatchResult.Success)
                continue;

            if(result == BatchResult.Error)
                corruptItems.AddRange(await ReprocessFailedBatch(batchChunk));
            
            if(result == BatchResult.TemporaryError)
                foreach(var item in batchChunk)
                    _queue.Enqueue(item);
        }

        return corruptItems;
    }

    private async Task<BatchResult> ProcessBatchAsync(IEnumerable<TWorkItem> batch)
    {
        using var scope = _serviceProvider.CreateScope();

        var processor = scope.ServiceProvider.GetRequiredService<IBatchProcessor<TWorkItem>>();

        _logger.LogInformation("Processing batch.");
        var result = await processor.ProcessBatchAsync(batch);
        _logger.LogDebug("Finished processing batch");

        return result;
    }

    private void ResetWaitTime()
    {
        _waitTime = _options.Value.PollWaitTime;
        _waitTimeIncreasesLeft = _options.Value.MaxIncreases;
    }

    private void IncreaseWaitTime()
    {
        _logger.LogDebug("Try increase waittime.");
        if (_waitTimeIncreasesLeft == 0)
        {
            _logger.LogDebug("Waittime already maxed out at {_waitTime}", _waitTime);
            return;
        }

        _waitTime = _waitTime * 2;
        _logger.LogDebug("Increasing waittime. Wattime now at {_waitTime}", _waitTime);
        _waitTimeIncreasesLeft--;
        _logger.LogDebug("{_waitTimeIncreasesLeft} steps left", _waitTimeIncreasesLeft);
    }
}
