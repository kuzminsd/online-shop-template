using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Application.Extensions;
using OnlineShop.Application.Models;
using OnlineShop.Domain.Models;
using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Application.Services;

public class OrderService(IOrderRepository orderRepository, IPaymentService paymentService): IOrderService
{
    public OrderInfo CreateOrder(Guid userId, decimal price)
    {
        var order = orderRepository.Create(userId, price);

        return ConvertToOrderInfo(order);
    }

    public OrderInfo GetOrderInfo(Guid orderId)
    {
        var order = orderRepository.Get(orderId);
        
        return ConvertToOrderInfo(order);
    }

    public StartPaymentResponse StartPayment(Guid orderId)
    {
        var payment = orderRepository.CreatePayment(orderId);
        paymentService.Pay(payment.Id);
        
        return new StartPaymentResponse(payment.CreatedAt.ToTimeMilliseconds(), payment.Id);
    }

    private static OrderInfo ConvertToOrderInfo(Order order)
    {
        return new OrderInfo
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status.ConvertToString(),
            TimeCreated = order.CreatedAt.ToTimeMilliseconds(),
            PaymentHistory = order.PaymentHistory
                .Where(x => x.PaymentFinishedAt is not null &&
                                    x.Status is PaymentStatus.Failed or PaymentStatus.Success)
                .Select(x => ConvertToPaymentInfo(x, order.TotalPrice)).ToList()
        };
    }

    private static PaymentInfo ConvertToPaymentInfo(Payment payment, decimal amount)
    {
        return new PaymentInfo(
            payment.PaymentFinishedAt!.Value.ToTimeMilliseconds(),
            payment.Status.ConvertToString(),
            (int)amount,
            payment.Id);
    }
}