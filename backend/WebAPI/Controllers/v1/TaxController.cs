using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/tax")]
    public class TaxController : GatewayControllerBase
    {
        private readonly ITaxService _taxService;

        public TaxController(ITaxService taxService)
        {
            _taxService = taxService;
        }

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig()
        {
            return ToResponse(await _taxService.GetConfigurationAsync());
        }

        [HttpPut("config")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> UpdateConfig([FromBody] UpdateTaxConfigurationDto request)
        {
            return ToResponse(await _taxService.UpdateConfigurationAsync(request));
        }

        [HttpPost("preview")]
        public async Task<IActionResult> PreviewTax([FromBody] TaxPreviewRequestDto request)
        {
            return ToResponse(await _taxService.PreviewTaxAsync(request));
        }

        [HttpPost("customers/{customerId}/exempt")]
        public async Task<IActionResult> SetCustomerExempt(Guid customerId, [FromBody] SetCustomerTaxExemptDto request)
        {
            return ToResponse(await _taxService.SetCustomerTaxExemptAsync(customerId, request));
        }

        [HttpPost("customers/{customerId}/tax-ids")]
        public async Task<IActionResult> AddTaxId(Guid customerId, [FromBody] CustomerTaxIdDto request)
        {
            return ToResponse(await _taxService.AddTaxIdAsync(customerId, request));
        }

        [HttpDelete("customers/{customerId}/tax-ids/{taxIdId}")]
        public async Task<IActionResult> RemoveTaxId(Guid customerId, Guid taxIdId)
        {
            return ToResponse(await _taxService.RemoveTaxIdAsync(customerId, taxIdId));
        }

        [HttpGet("report")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetTaxReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            return ToResponse(await _taxService.GetTaxReportAsync(from, to));
        }

        [HttpGet("rates")]
        public async Task<IActionResult> GetTaxRates([FromQuery] string country)
        {
            return ToResponse(await _taxService.GetTaxRatesAsync(country));
        }
    }
}
