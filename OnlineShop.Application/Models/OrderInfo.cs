namespace OnlineShop.Application.Models;

public class OrderInfo
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }

    public string Status { get; set; } = null!;
    
    public long TimeCreated { get; set; }
    
    public IEnumerable<PaymentInfo> PaymentHistory { get; set; } = null!;

    public Dictionary<Guid, int> ItemsMap { get; set; } = new();
}