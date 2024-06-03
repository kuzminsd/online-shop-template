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

    public async Task Pay(Guid paymentId, CancellationToken cancellationToken)
    {
        try
        {
            var url =
                $"{httpClientOptions.Value.BaseAddress}external/process?serviceName={ServiceName}&accountName={AccountName}&transactionId={paymentId}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            var response = await httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                await orderRepository.SetPaymentSuccess(paymentId, cancellationToken);

            }
            else
            {
                await orderRepository.SetPaymentFailed(paymentId, cancellationToken);
            }
        }
        catch
        {
            await orderRepository.SetPaymentFailed(paymentId, cancellationToken);
        }
    }
}