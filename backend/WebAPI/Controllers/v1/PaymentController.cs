using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/payments")]
    public class PaymentController : GatewayControllerBase
    {
        private readonly IPaymentGateway _paymentGateway;

        public PaymentController(IPaymentGateway paymentGateway)
        {
            _paymentGateway = paymentGateway;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] CreateCheckoutDto request)
        {
            return ToResponse(await _paymentGateway.CreateCheckoutSessionAsync(request));
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentDto request)
        {
            return ToResponse(await _paymentGateway.CreatePaymentIntentAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            return ToResponse(await _paymentGateway.GetPaymentAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> ListPayments([FromQuery] PaymentFilterDto filter)
        {
            return ToResponse(await _paymentGateway.ListPaymentsAsync(filter));
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] string period = "30d")
        {
            return ToResponse(await _paymentGateway.GetPaymentAnalyticsAsync(period));
        }
    }
}
