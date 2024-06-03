using OnlineShop.Application.Models;

namespace OnlineShop.Application.Contracts;

public interface IOrderService
{
    Task<OrderInfo> CreateOrder(Guid userId, decimal price, CancellationToken cancellationToken);
    
    Task<OrderInfo> GetOrderInfo(Guid orderId, CancellationToken cancellationToken);
    
    Task<StartPaymentResponse> StartPayment(Guid orderId, CancellationToken cancellationToken);
}