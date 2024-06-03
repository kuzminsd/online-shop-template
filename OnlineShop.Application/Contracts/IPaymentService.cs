namespace OnlineShop.Application.Contracts;

public interface IPaymentService
{
    Task Pay(Guid paymentId, CancellationToken cancellationToken);
}