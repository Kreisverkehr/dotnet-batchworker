using Kreisverkehr.Extensions.Hosting.BatchWorker;

namespace sample;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBatchQueue<Stone> _queue;
    private readonly Random _random = new();

    public Worker(ILogger<Worker> logger, IBatchQueue<Stone> queue)
    {
        _logger = logger;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Generating stones every second");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
            _queue.Enqueue(GenerateStone());
        }
    }

    private Stone GenerateStone()
    {
        decimal stoneWeight = (decimal)_random.NextDouble() * 10_000;
        var stoneCategory = stoneWeight switch {
            > 9000 => StoneCategory.Heavy,
            < 5000 => StoneCategory.Light,
            _      => StoneCategory.Medium
        };

        return new(stoneWeight, stoneCategory);
    }
}
