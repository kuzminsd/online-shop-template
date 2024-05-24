using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Domain.Models;
using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Persistence.Repositories;

public class OrderRepository(OnlineShopDbContext dbContext) : IOrderRepository
{
    public Order Create(Guid userId, decimal price)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            TotalPrice = price,
            UserId = userId
        };

        dbContext.Orders.Add(order);
        dbContext.SaveChanges();
        return order;
    }

    public Order Get(Guid orderId)
    {
        return dbContext.Orders
            .Include(x => x.PaymentHistory)
            .First(x => x.Id == orderId);
    }

    public Payment CreatePayment(Guid orderId)
    {
        var order = dbContext.Orders.First(x => x.Id == orderId);

        order.Status = OrderStatus.PaymentInProgress;
        
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Created
        };

        dbContext.Payments.Add(payment);
        dbContext.SaveChanges();

        return payment;
    }

    public Payment SetPaymentSuccess(Guid paymentId)
    {
        var payment = dbContext.Payments
            .Include(x => x.Order)
            .First(x => x.Id == paymentId);

        payment.Status = PaymentStatus.Success;
        payment.PaymentFinishedAt = DateTime.UtcNow;
        payment.Order.Status = OrderStatus.Payed;
        
        dbContext.SaveChanges();

        return payment;
    }

    public Payment SetPaymentFailed(Guid paymentId)
    {
        var payment = dbContext.Payments.First(x => x.Id == paymentId);

        payment.Status = PaymentStatus.Failed;
        payment.PaymentFinishedAt = DateTime.UtcNow;
        
        dbContext.SaveChanges();

        return payment;
    }
}