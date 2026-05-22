using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IPaymentGateway
    {
        Task<GatewayResponseWrapper<CheckoutResponseDto>> CreateCheckoutSessionAsync(CreateCheckoutDto request);
        Task<GatewayResponseWrapper<PaymentIntentResponseDto>> CreatePaymentIntentAsync(CreatePaymentIntentDto request);
        Task<GatewayResponseWrapper<PaymentResponseDto>> GetPaymentAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<PaymentResponseDto>> ListPaymentsAsync(PaymentFilterDto filter);
        Task<GatewayResponseWrapper<PaymentAnalyticsDto>> GetPaymentAnalyticsAsync(string period);
    }
}
