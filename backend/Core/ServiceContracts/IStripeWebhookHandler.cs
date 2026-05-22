using Core.Utils;
using Stripe;

namespace Core.ServiceContracts
{
    public interface IStripeWebhookHandler
    {
        Task<GatewayResponseWrapper<bool>> ProcessAsync(Event stripeEvent, Guid tenantId);
    }
}
