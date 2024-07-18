using System.Threading.Channels;

namespace OnlineShop.Application.Contracts;

public interface IPaymentsProcessingService
{
    public Task ReadPaymentsForProcessing(ChannelWriter<Guid> destinationQueue, CancellationToken cancellationToken);
}