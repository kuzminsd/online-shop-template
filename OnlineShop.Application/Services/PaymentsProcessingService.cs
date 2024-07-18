using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;

namespace OnlineShop.Application.Services;

public sealed class PaymentsProcessingService(IOrderRepository orderRepository, ILogger<PaymentsProcessingService> logger)
    : IPaymentsProcessingService
{
    public async Task ReadPaymentsForProcessing(ChannelWriter<Guid> destinationQueue, CancellationToken cancellationToken)
    {
        var paymentsForProcessing = await orderRepository.GetPaymentsForProcessing(cancellationToken);
        foreach (var payment in paymentsForProcessing)
        {
            await destinationQueue.WriteAsync(payment.Id, cancellationToken);
        }
    }
}