using Microsoft.EntityFrameworkCore;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Domain.Models;
using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Persistence.Repositories;

public class OrderRepository(OnlineShopDbContext dbContext) : IOrderRepository
{
    public async Task<Order> Create(Guid userId, decimal price, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            TotalPrice = price,
            UserId = userId
        };

        await dbContext.Orders.AddAsync(order, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order> Get(Guid orderId, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Include(x => x.PaymentHistory)
            .FirstAsync(x => x.Id == orderId, cancellationToken);
    }

    public async Task<Payment> CreatePayment(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstAsync(x => x.Id == orderId, cancellationToken);

        order.Status = OrderStatus.PaymentInProgress;
        
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            Status = PaymentStatus.Created
        };

        await dbContext.Payments.AddAsync(payment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return payment;
    }

    public async Task<Payment> SetPaymentProcessing(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .Include(x => x.Order)
            .FirstAsync(x => x.Id == paymentId, cancellationToken);

        payment.Status = PaymentStatus.Processing;
        payment.PaymentFinishedAt = DateTime.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return payment;
    }

    public async Task<Payment> SetPaymentSuccess(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments
            .Include(x => x.Order)
            .FirstAsync(x => x.Id == paymentId, cancellationToken);

        payment.Status = PaymentStatus.Success;
        payment.PaymentFinishedAt = DateTime.UtcNow;
        payment.Order.Status = OrderStatus.Payed;
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return payment;
    }

    public async Task<Payment> SetPaymentFailed(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await dbContext.Payments.FirstAsync(x => x.Id == paymentId, cancellationToken);

        payment.Status = PaymentStatus.Failed;
        payment.PaymentFinishedAt = DateTime.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return payment;
    }

    public async Task<IReadOnlyCollection<Payment>> GetPaymentsForProcessing(CancellationToken cancellationToken)
    {
        return await dbContext
            .Payments
            .Where(x => x.Status == PaymentStatus.Created)
            .OrderBy(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}