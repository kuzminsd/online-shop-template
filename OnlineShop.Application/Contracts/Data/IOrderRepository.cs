using OnlineShop.Domain.Models;

namespace OnlineShop.Application.Contracts.Data;

public interface IOrderRepository
{
    Task<Order> Create(Guid userId, decimal price, CancellationToken cancellationToken);

    Task<Order> Get(Guid orderId, CancellationToken cancellationToken);

    Task<Payment> CreatePayment(Guid orderId, CancellationToken cancellationToken);
    
    Task<Payment> SetPaymentSuccess(Guid paymentId, CancellationToken cancellationToken);
    
    Task<Payment> SetPaymentFailed(Guid paymentId, CancellationToken cancellationToken);
}