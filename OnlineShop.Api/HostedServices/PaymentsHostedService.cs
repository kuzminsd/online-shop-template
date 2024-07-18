using System.Threading.Channels;
using OnlineShop.Application.Contracts;

namespace OnlineShop.Api.HostedServices;

public class PaymentsHostedService : BackgroundService
{
    private readonly ChannelReader<Guid> _channelReader;
    private readonly ChannelWriter<Guid> _channelWriter;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentsHostedService> _logger;

    public PaymentsHostedService(IServiceProvider serviceProvider, ILogger<PaymentsHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var channel = Channel.CreateUnbounded<Guid>();
        _channelReader = channel.Reader;
        _channelWriter = channel.Writer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(ReadPaymentsForProcessing(stoppingToken), ProcessPayments(stoppingToken));
    }

    private async Task ReadPaymentsForProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var awaitingTask = Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                using var scope = _serviceProvider.CreateScope();
                var paymentsProcessingService =
                    scope.ServiceProvider.GetRequiredService<IPaymentsProcessingService>();

                await Task.WhenAll(paymentsProcessingService.ReadPaymentsForProcessing(_channelWriter, stoppingToken),
                    awaitingTask);
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("PaymentsHostedService: Task was canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error occured: {message}", ex.Message);
            }
        }
    }

    private async Task ProcessPayments(CancellationToken cancellationToken)
    {
        while (await _channelReader.WaitToReadAsync(cancellationToken))
        {
            while (_channelReader.TryRead(out var paymentId))
            {
                try
                {
                    await Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var paymentService =
                                scope.ServiceProvider.GetRequiredService<IPaymentService>();

                            await paymentService.Pay(paymentId, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Payment {id} failed: {message}", paymentId, ex.Message);
                        }
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Payment {id} failed: {message}", paymentId, ex.Message);
                }
            }
        }
    }
}