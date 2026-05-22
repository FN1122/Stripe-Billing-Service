using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ICustomerService
    {
        Task<GatewayResponseWrapper<CustomerResponseDto>> CreateAsync(CreateCustomerDto request);
        Task<GatewayResponseWrapper<CustomerDetailResponseDto>> GetAsync(Guid id);
        Task<GatewayResponseWrapper<CustomerResponseDto>> GetByExternalRefAsync(string externalRefId);
        Task<GatewayResponseWrapper<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerDto request);
        Task<GatewayPaginatedListResponseWrapper<CustomerResponseDto>> ListAsync(CustomerFilterDto filter);
        Task<GatewayResponseWrapper<string>> CreatePortalSessionAsync(Guid id);
    }
}
