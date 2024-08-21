using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineShop.Application.Contracts;
using OnlineShop.Application.Contracts.Data;
using OnlineShop.Application.Options;

namespace OnlineShop.Application.Services;

public class PaymentService(
    IOrderRepository orderRepository,
    HttpClient httpClient,
    IOptions<ExternalPaymentServiceOptions> httpClientOptions,
    ILogger<PaymentService> logger) : IPaymentService
{
    private const string ServiceName = "test";
    private const string AccountName = "test-account";

    public async Task Pay(Guid paymentId, CancellationToken cancellationToken)
    {
        try
        {
            await orderRepository.SetPaymentProcessing(paymentId, cancellationToken);
            
            var url =
                $"{httpClientOptions.Value.BaseAddress}external/process?serviceName={ServiceName}&accountName={AccountName}&transactionId={paymentId}&paymentId={paymentId}";
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
        catch(Exception ex)
        {
            logger.LogError("Payment {paymentId} failed: {message}", paymentId, ex.Message);
            await orderRepository.SetPaymentFailed(paymentId, cancellationToken);
        }
    }

    public async Task ProcessPayments(CancellationToken cancellationToken)
    {
        try
        {
            var payments = await orderRepository.GetPaymentsForProcessing(cancellationToken);

            foreach (var payment in payments)
            {
                await Pay(payment.Id, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Payments processing failed: {message}", ex.Message);
        }
    }
}