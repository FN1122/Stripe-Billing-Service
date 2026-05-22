using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IInvoiceService
    {
        Task<GatewayResponseWrapper<InvoiceResponseDto>> GetAsync(Guid id);
        Task<GatewayPaginatedListResponseWrapper<InvoiceResponseDto>> ListAsync(InvoiceFilterDto filter);
        Task<GatewayResponseWrapper<string>> GetPdfUrlAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> VoidAsync(Guid id);
        Task<GatewayResponseWrapper<bool>> SendEmailAsync(Guid id);
        Task SyncFromStripeAsync(string stripeInvoiceId, Guid tenantId);
    }
}
