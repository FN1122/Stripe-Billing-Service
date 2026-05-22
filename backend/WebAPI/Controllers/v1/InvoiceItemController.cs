using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/invoice-items")]
    public class InvoiceItemController : GatewayControllerBase
    {
        private readonly IInvoiceItemService _invoiceItemService;

        public InvoiceItemController(IInvoiceItemService invoiceItemService)
        {
            _invoiceItemService = invoiceItemService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInvoiceItemDto request)
        {
            return ToResponse(await _invoiceItemService.CreateAsync(request));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] Guid? customerId, [FromQuery] Guid? subscriptionId)
        {
            return ToResponse(await _invoiceItemService.ListAsync(customerId, subscriptionId));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return ToResponse(await _invoiceItemService.DeleteAsync(id));
        }

        [HttpGet("upcoming/{subscriptionId}")]
        public async Task<IActionResult> GetUpcomingInvoice(Guid subscriptionId)
        {
            return ToResponse(await _invoiceItemService.GetUpcomingInvoiceAsync(subscriptionId));
        }
    }
}
