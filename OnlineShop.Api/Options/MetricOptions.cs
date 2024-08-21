namespace OnlineShop.Api.Options;

public class MetricOptions
{
    public bool UseDefaultMetrics { get; set; } = false;
    public bool UseDebuggingMetrics { get; set; } = false;
    public TimeSpan RecycleEvery { get; set; } = TimeSpan.FromDays(1);
}