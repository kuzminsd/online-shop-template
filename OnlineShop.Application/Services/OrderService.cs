using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Application.Extensions;
using OnlineShop.Application.Models;
using OnlineShop.Domain.Models;
using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Application.Services;

public class OrderService(IOrderRepository orderRepository, IPaymentService paymentService): IOrderService
{
    public async Task<OrderInfo> CreateOrder(Guid userId, decimal price, CancellationToken cancellationToken)
    {
        var order = await orderRepository.Create(userId, price, cancellationToken);

        return ConvertToOrderInfo(order);
    }

    public async Task<OrderInfo> GetOrderInfo(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orderRepository.Get(orderId, cancellationToken);
        
        return ConvertToOrderInfo(order);
    }

    public async Task<StartPaymentResponse> StartPayment(Guid orderId, CancellationToken cancellationToken)
    {
        var payment = await orderRepository.CreatePayment(orderId, cancellationToken);
        
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