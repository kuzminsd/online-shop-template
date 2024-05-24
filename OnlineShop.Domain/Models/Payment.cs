using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Domain.Models;

public class Payment
{
    public Guid Id { get; set; }
    
    public Guid OrderId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? PaymentFinishedAt { get; set; }
    
    public PaymentStatus Status { get; set; }

    public Order Order { get; set; } = null!;
}