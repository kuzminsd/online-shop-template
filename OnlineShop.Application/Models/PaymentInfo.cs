namespace OnlineShop.Application.Models;

public record PaymentInfo(long Timestamp, string Status, int Amount, Guid TransactionId);