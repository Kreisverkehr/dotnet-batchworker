using Kreisverkehr.Extensions.Hosting.BatchWorker.Internal;
using Kreisverkehr.Extensions.Hosting.BatchWorker.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kreisverkehr.Extensions.Hosting.BatchWorker;

public static class ServiceCollectionExtensions
{
    public const string CONFIG_SECTION = "BatchWorker";

    public static IServiceCollection AddBatchWorker<TProcessor, TWorkItem>(this IServiceCollection services)
        where TProcessor : class, IBatchProcessor<TWorkItem>
        => services.AddBatchWorker<TProcessor, TWorkItem>(CONFIG_SECTION);

    public static IServiceCollection AddBatchWorker<TProcessor, TWorkItem>(this IServiceCollection services, string configSectionName)
        where TProcessor : class, IBatchProcessor<TWorkItem>
        => services.AddBatchWorker<TProcessor, TWorkItem, DefaultBatchQueueFactory<TWorkItem>>(configSectionName);

    public static IServiceCollection AddBatchWorker<TProcessor, TWorkItem, TQueueFactory>(this IServiceCollection services)
        where TProcessor : class, IBatchProcessor<TWorkItem>
        where TQueueFactory : class, IBatchQueueFactory<TWorkItem>
        => services.AddBatchWorker<TProcessor, TWorkItem, DefaultBatchQueueFactory<TWorkItem>>(CONFIG_SECTION);

    public static IServiceCollection AddBatchWorker<TProcessor, TWorkItem, TQueueFactory>(this IServiceCollection services, string configSectionName)
        where TProcessor : class, IBatchProcessor<TWorkItem>
        where TQueueFactory : class, IBatchQueueFactory<TWorkItem>
        => services
            .AddHostedService<DefaultBatchWorker<TWorkItem>>()
            .AddSingleton<IBatchQueueFactory<TWorkItem>, TQueueFactory>()
            .AddSingleton<IBatchQueue<TWorkItem>>(sp => sp.GetRequiredService<IBatchQueueFactory<TWorkItem>>().CreateQueue())
            .AddScoped<IBatchProcessor<TWorkItem>, TProcessor>()
            .AddOptions<BatchProcessorOptions<TWorkItem>>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection($"{configSectionName}:{typeof(TProcessor).FullName}").Bind(options)
                ).Services
            .AddOptions<FixedBatchSizeQueueOptions<TWorkItem>>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection($"{configSectionName}:{typeof(TProcessor).FullName}:Queue").Bind(options)
                ).Services
            .AddOptions<FixedTimeQueueOptions<TWorkItem>>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection($"{configSectionName}:{typeof(TProcessor).FullName}:Queue").Bind(options)
                ).Services
            .AddOptions<MaxSizeMaxIntervalQueueOptions<TWorkItem>>()
                .Configure<IConfiguration>((options, configuration) =>
                    configuration.GetSection($"{configSectionName}:{typeof(TProcessor).FullName}:Queue").Bind(options)
                ).Services
        ;
}