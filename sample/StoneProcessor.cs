using Kreisverkehr.Extensions.Hosting.BatchWorker;

namespace sample;

public enum StoneCategory { Light, Medium, Heavy }
public record Stone(decimal Weight, StoneCategory Category);

public class StoneProcessor : IBatchProcessor<Stone>
{
    private readonly ILogger<StoneProcessor> _logger;

    public StoneProcessor(ILogger<StoneProcessor> logger)
    {
        _logger = logger;
    }

    public Task<BatchResult> ProcessBatchAsync(IEnumerable<Stone> batch)
    {
        _logger.LogInformation("Processing {Count} stones with a total weight of {Sum}",
            batch.Count(),
            batch.Sum(s => s.Weight));

        if(batch.Select(s => s.Category).Contains(StoneCategory.Heavy))
        {
            _logger.LogError("Can't pickup heavy stones!");
            return Task.FromResult(BatchResult.Error);
        }

        if(batch.Select(s => s.Category).Contains(StoneCategory.Medium)
            && DateTime.Now.Minute % 2 == 0)
        {
            _logger.LogWarning("Can't pickup medium stones right now.");
            return Task.FromResult(BatchResult.TemporaryError);
        }

        int lightStonesCount = batch.Count(s => s.Category == StoneCategory.Light);
        int mediumStonesCount = batch.Count(s => s.Category == StoneCategory.Medium);
        _logger.LogInformation("Picked up {lightStonesCount} light stones and {mediumStonesCount} medium ones.",
            lightStonesCount,
            mediumStonesCount);
        return Task.FromResult(BatchResult.Success);
    }
}