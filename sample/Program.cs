using Kreisverkehr.Extensions.Hosting.BatchWorker;
using sample;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddBatchWorker<StoneProcessor, Stone>();
    })
    .Build();

await host.RunAsync();
