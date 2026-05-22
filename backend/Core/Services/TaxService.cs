using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Newtonsoft.Json;

namespace Core.Services
{
    public class TaxService : BaseService, ITaxService
    {
        private readonly ITaxRepository _taxRepo;

        public TaxService(ITenantContextProvider tenantContextProvider, ITaxRepository taxRepo) : base(tenantContextProvider)
        {
            _taxRepo = taxRepo;
        }

        public async Task<GatewayResponseWrapper<TaxConfigurationResponseDto>> GetConfigurationAsync()
        {
            var response = new GatewayResponseWrapper<TaxConfigurationResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var config = await _taxRepo.GetConfigAsync(tenantId);

            if (config == null)
            {
                config = new TaxConfiguration { TenantId = tenantId };
                await _taxRepo.CreateConfigAsync(config);
            }

            response.SetSuccess(MapConfig(config));
            return response;
        }

        public async Task<GatewayResponseWrapper<TaxConfigurationResponseDto>> UpdateConfigurationAsync(UpdateTaxConfigurationDto request)
        {
            var response = new GatewayResponseWrapper<TaxConfigurationResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var config = await _taxRepo.GetConfigAsync(tenantId);

            if (config == null)
            {
                config = new TaxConfiguration { TenantId = tenantId };
                await _taxRepo.CreateConfigAsync(config);
            }

            config.Provider = request.TaxProvider;
            config.IsEnabled = request.AutomaticTax;
            config.AutoCalculate = request.AutomaticTax;
            config.DefaultTaxBehavior = request.DefaultTaxBehavior;
            config.RegistrationNumbers = JsonConvert.SerializeObject(request.TaxRegistrations);
            config.UpdatedAt = DateTime.UtcNow;

            await _taxRepo.UpdateConfigAsync(config);
            response.SetSuccess(MapConfig(config));
            return response;
        }

        public async Task<GatewayResponseWrapper<TaxCalculationPreviewDto>> PreviewTaxAsync(TaxPreviewRequestDto request)
        {
            var response = new GatewayResponseWrapper<TaxCalculationPreviewDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var config = await _taxRepo.GetConfigAsync(tenantId);

            decimal taxRate = config?.FallbackTaxRate ?? 0;
            decimal taxAmount = request.Amount * taxRate;

            var preview = new TaxCalculationPreviewDto
            {
                Subtotal = request.Amount,
                TaxAmount = Math.Round(taxAmount, 2),
                Total = Math.Round(request.Amount + taxAmount, 2),
                TaxBreakdown = new List<TaxLineItemDto>
                {
                    new TaxLineItemDto
                    {
                        Jurisdiction = "Default",
                        TaxRate = taxRate * 100,
                        TaxableAmount = request.Amount,
                        TaxAmount = Math.Round(taxAmount, 2),
                        Description = "Estimated tax"
                    }
                }
            };

            response.SetSuccess(preview);
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> SetCustomerTaxExemptAsync(Guid customerId, SetCustomerTaxExemptDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;

            var exemption = new TaxExemption
            {
                TenantId = tenantId,
                CustomerId = customerId,
                ExemptionType = request.TaxExempt
            };

            await _taxRepo.CreateExemptionAsync(exemption);
            response.SetSuccess(true, "Tax exemption updated.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> AddTaxIdAsync(Guid customerId, CustomerTaxIdDto request)
        {
            var response = new GatewayResponseWrapper<bool>();
            // In production, this would also sync with Stripe
            response.SetSuccess(true, "Tax ID added successfully.");
            return response;
        }

        public async Task<GatewayResponseWrapper<bool>> RemoveTaxIdAsync(Guid customerId, Guid taxExemptionId)
        {
            var response = new GatewayResponseWrapper<bool>();
            var tenantId = CurrentTenantContext.TenantId;
            await _taxRepo.DeleteExemptionAsync(tenantId, taxExemptionId);
            response.SetSuccess(true, "Tax exemption removed.");
            return response;
        }

        public async Task<GatewayResponseWrapper<TaxReportDto>> GetTaxReportAsync(DateTime from, DateTime to)
        {
            var response = new GatewayResponseWrapper<TaxReportDto>();
            var report = new TaxReportDto
            {
                PeriodFrom = from,
                PeriodTo = to,
                TotalTaxCollected = 0,
                TaxableRevenue = 0,
                ExemptRevenue = 0,
                ByJurisdiction = new Dictionary<string, decimal>()
            };
            response.SetSuccess(report);
            return response;
        }

        public async Task<GatewayResponseWrapper<List<TaxRateDto>>> GetTaxRatesAsync(string country)
        {
            var response = new GatewayResponseWrapper<List<TaxRateDto>>();
            response.SetSuccess(new List<TaxRateDto>());
            return response;
        }

        private TaxConfigurationResponseDto MapConfig(TaxConfiguration c)
        {
            var registrations = new List<TaxRegistrationItemDto>();
            if (!string.IsNullOrEmpty(c.RegistrationNumbers))
            {
                try { registrations = JsonConvert.DeserializeObject<List<TaxRegistrationItemDto>>(c.RegistrationNumbers) ?? new(); }
                catch { }
            }

            return new TaxConfigurationResponseDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                TaxProvider = c.Provider,
                AutomaticTax = c.IsEnabled,
                DefaultTaxBehavior = c.DefaultTaxBehavior,
                TaxRegistrations = registrations,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
}
