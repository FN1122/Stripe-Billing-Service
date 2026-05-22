using Core.Infrastructure;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/portal")]
    [Authorize]
    public class PortalController : GatewayControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IInvoiceService _invoiceService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly BillingDbContext _db;

        public PortalController(ICustomerService customerService, IInvoiceService invoiceService, ISubscriptionService subscriptionService, IPaymentGateway paymentGateway, BillingDbContext db)
        {
            _customerService = customerService;
            _invoiceService = invoiceService;
            _subscriptionService = subscriptionService;
            _paymentGateway = paymentGateway;
            _db = db;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetBillingSummary()
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == null) return Ok(new Core.Utils.GatewayResponseWrapper<object> { IsValid = false, Message = "Customer profile not found." });
            return ToResponse(await _customerService.GetAsync(customerId.Value));
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return ToResponse(await _paymentGateway.ListPaymentsAsync(new Core.Dtos.Requests.PaymentFilterDto { Page = page, PageSize = pageSize }));
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return ToResponse(await _subscriptionService.ListAsync(new Core.Dtos.Requests.SubscriptionFilterDto { Page = page, PageSize = pageSize }));
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return ToResponse(await _invoiceService.ListAsync(new Core.Dtos.Requests.InvoiceFilterDto { Page = page, PageSize = pageSize }));
        }

        [HttpGet("invoices/{id}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(Guid id)
        {
            return ToResponse(await _invoiceService.GetPdfUrlAsync(id));
        }

        private async Task<Guid?> GetCurrentCustomerId()
        {
            var userId = GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.TenantId == user.TenantId && c.Email == user.Email);
            return customer?.Id;
        }
    }
}
