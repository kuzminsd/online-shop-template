using OnlineShop.Domain.Models;
using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Application.Contracts.Data;

public interface IOrderRepository
{
    Order Create(Guid userId, decimal price);

    Order Get(Guid orderId);

    Payment CreatePayment(Guid orderId);
    
    Payment SetPaymentSuccess(Guid paymentId);
    
    Payment SetPaymentFailed(Guid paymentId);
}