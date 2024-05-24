using OnlineShop.Domain.ValueTypes;

namespace OnlineShop.Domain.Models;

public class Order
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public Guid UserId { get; set; }
    
    public OrderStatus Status { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public List<Payment> PaymentHistory { get; set; } = new();
}