using OnlineShop.Application.Models;

namespace OnlineShop.Application.Contracts;

public interface IOrderService
{
    OrderInfo CreateOrder(Guid userId, decimal price);
    
    OrderInfo GetOrderInfo(Guid orderId);
    
    StartPaymentResponse StartPayment(Guid orderId);
}