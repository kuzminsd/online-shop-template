namespace OnlineShop.Application.Contracts;

public interface IPaymentService
{
    void Pay(Guid paymentId);
}