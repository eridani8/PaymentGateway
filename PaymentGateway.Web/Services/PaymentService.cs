using PaymentGateway.Shared.DTOs.Payment;
using PaymentGateway.Web.Interfaces;

namespace PaymentGateway.Web.Services;

public class PaymentService(
    IHttpClientFactory factory,
    ILogger<RequisiteService> logger) : ServiceBase(factory, logger), IPaymentService
{
    private const string ApiEndpoint = "Payment";
    
    public async Task<Response> ManualConfirmPayment(Guid id)
    {
        return await PostRequest($"{ApiEndpoint}/ManualConfirmPayment", new PaymentManualConfirmDto()
        {
            PaymentId = id
        });
    }
}