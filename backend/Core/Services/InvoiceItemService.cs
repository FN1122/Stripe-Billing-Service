using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.ServiceContracts;
using Core.Utils;

namespace Core.Services
{
    public class InvoiceItemService : BaseService, IInvoiceItemService
    {
        public InvoiceItemService(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider) { }

        public async Task<GatewayResponseWrapper<InvoiceItemResponseDto>> CreateAsync(CreateInvoiceItemDto request)
        {
            var response = new GatewayResponseWrapper<InvoiceItemResponseDto>();
            var item = new InvoiceItemResponseDto
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                SubscriptionId = request.SubscriptionId,
                Amount = request.Amount,
                Currency = request.Currency,
                Description = request.Description,
                Quantity = request.Quantity,
                UnitAmount = request.Amount / request.Quantity,
                CreatedAt = DateTime.UtcNow
            };
            response.SetSuccess(item);
            return response;
        }

        public async Task<GatewayResponseWrapper<List<InvoiceItemResponseDto>>> ListAsync(Guid? customerId, Guid? subscriptionId)
        {
            var response = new GatewayResponseWrapper<List<InvoiceItemResponseDto>>();
            response.SetSuccess(new List<InvoiceItemResponseDto>());
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id)
        {
            var response = new GatewayResponseWrapper<bool>();
            response.SetSuccess(true, "Invoice item deleted.");
            return response;
        }

        public async Task<GatewayResponseWrapper<InvoiceResponseDto>> GetUpcomingInvoiceAsync(Guid subscriptionId)
        {
            var response = new GatewayResponseWrapper<InvoiceResponseDto>();
            response.SetError("Upcoming invoice preview requires Stripe integration.", 501);
            return response;
        }
    }
}
