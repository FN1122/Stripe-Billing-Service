using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IInvoiceItemService
    {
        Task<GatewayResponseWrapper<InvoiceItemResponseDto>> CreateAsync(CreateInvoiceItemDto request);
        Task<GatewayResponseWrapper<List<InvoiceItemResponseDto>>> ListAsync(Guid? customerId, Guid? subscriptionId);
        Task<GatewayResponseWrapper<bool>> DeleteAsync(Guid id);
        Task<GatewayResponseWrapper<InvoiceResponseDto>> GetUpcomingInvoiceAsync(Guid subscriptionId);
    }
}
