using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/invoices")]
    public class InvoiceController : GatewayControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] InvoiceFilterDto filter)
        {
            return ToResponse(await _invoiceService.ListAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _invoiceService.GetAsync(id));
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(Guid id)
        {
            return ToResponse(await _invoiceService.GetPdfUrlAsync(id));
        }

        [HttpPost("{id}/void")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Void(Guid id)
        {
            return ToResponse(await _invoiceService.VoidAsync(id));
        }

        [HttpPost("{id}/send")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> SendEmail(Guid id)
        {
            return ToResponse(await _invoiceService.SendEmailAsync(id));
        }
    }
}
