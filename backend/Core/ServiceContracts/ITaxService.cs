using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface ITaxService
    {
        Task<GatewayResponseWrapper<TaxConfigurationResponseDto>> GetConfigurationAsync();
        Task<GatewayResponseWrapper<TaxConfigurationResponseDto>> UpdateConfigurationAsync(UpdateTaxConfigurationDto request);
        Task<GatewayResponseWrapper<TaxCalculationPreviewDto>> PreviewTaxAsync(TaxPreviewRequestDto request);
        Task<GatewayResponseWrapper<bool>> SetCustomerTaxExemptAsync(Guid customerId, SetCustomerTaxExemptDto request);
        Task<GatewayResponseWrapper<bool>> AddTaxIdAsync(Guid customerId, CustomerTaxIdDto request);
        Task<GatewayResponseWrapper<bool>> RemoveTaxIdAsync(Guid customerId, Guid taxExemptionId);
        Task<GatewayResponseWrapper<TaxReportDto>> GetTaxReportAsync(DateTime from, DateTime to);
        Task<GatewayResponseWrapper<List<TaxRateDto>>> GetTaxRatesAsync(string country);
    }
}
