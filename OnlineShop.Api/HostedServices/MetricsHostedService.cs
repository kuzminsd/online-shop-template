using Microsoft.Extensions.Options;
using OnlineShop.Api.Options;
using Prometheus.DotNetRuntime;

namespace OnlineShop.Api.HostedServices;

public class MetricsHostedService(IOptions<MetricOptions> metricOptions, ILogger<MetricsHostedService> logger)
    : IHostedService
{
    private IDisposable? _collector;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _collector = CreateCollector(
            metricOptions.Value.RecycleEvery,
            metricOptions.Value.UseDefaultMetrics,
            metricOptions.Value.UseDebuggingMetrics);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _collector?.Dispose();

        return Task.CompletedTask;
    }

    private IDisposable CreateCollector(
        TimeSpan recycleEvery,
        bool useDefaultMetrics = true,
        bool useDebuggingMetrics = false)
    {
        var dotnetRuntimeStatsBuilder = DotNetRuntimeStatsBuilder.Default();

        if (!useDefaultMetrics)
        {
            dotnetRuntimeStatsBuilder = DotNetRuntimeStatsBuilder.Customize()
                .WithContentionStats(CaptureLevel.Informational)
                .WithGcStats(CaptureLevel.Verbose)
                .WithThreadPoolStats(CaptureLevel.Informational)
                .WithExceptionStats(CaptureLevel.Errors)
                .WithJitStats();
        }

        dotnetRuntimeStatsBuilder
            .RecycleCollectorsEvery(recycleEvery)
            .WithErrorHandler(
                ex => logger.LogError(ex, "Unexpected exception occurred in prometheus-net.DotNetRuntime"));

        if (useDebuggingMetrics)
        {
            logger.LogInformation("Using debugging metrics.");
            dotnetRuntimeStatsBuilder.WithDebuggingMetrics(true);
        }

        logger.LogInformation("Starting prometheus-net.DotNetRuntime...");

        return dotnetRuntimeStatsBuilder.StartCollecting();
    }
}