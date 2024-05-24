using System.Net;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Application.Options;

namespace OnlineShop.Application.Services;

public class PaymentService(
    IOrderRepository orderRepository,
    HttpClient httpClient,
    IOptions<ExternalPaymentServiceOptions> httpClientOptions) : IPaymentService
{
    private const string ServiceName = "test";
    private const string AccountName = "default-1";

    public void Pay(Guid paymentId)
    {
        try
        {
            var url =
                $"{httpClientOptions.Value.BaseAddress}external/process?serviceName={ServiceName}&accountName={AccountName}&transactionId={paymentId}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            var response = httpClient.Send(httpRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                orderRepository.SetPaymentSuccess(paymentId);

            }
            else
            {
                orderRepository.SetPaymentFailed(paymentId);
            }
        }
        catch
        {
            orderRepository.SetPaymentFailed(paymentId);
        }
    }
}