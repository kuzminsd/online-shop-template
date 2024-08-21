using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;

namespace OnlineShop.Api.HostedServices;

public class PaymentsHostedService(IServiceProvider serviceProvider, ILogger<PaymentsHostedService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var awaitingTask = Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
            var processPaymentsTask = ProcessPayments(stoppingToken);
            await Task.WhenAll(processPaymentsTask, awaitingTask);
        }
    }
    

    private async Task ProcessPayments(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            
            var payments = await orderRepository.GetPaymentsForProcessing(cancellationToken);

            foreach (var payment in payments)
            {
                var paymentService =
                    scope.ServiceProvider.GetRequiredService<IPaymentService>();
            
                await paymentService.Pay(payment.Id, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Payments processing failed: {message}", ex.Message);
        }
    }
}
