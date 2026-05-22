using Core.Infrastructure;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/webhooks")]
    [AllowAnonymous]
    public class WebhookInboundController : ControllerBase
    {
        private readonly BillingDbContext _db;
        private readonly IStripeWebhookHandler _webhookHandler;

        public WebhookInboundController(BillingDbContext db, IStripeWebhookHandler webhookHandler)
        {
            _db = db;
            _webhookHandler = webhookHandler;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            // Try each tenant's webhook secret to find the matching one
            var tenants = await _db.Tenants.Where(t => t.IsActive && t.StripeWebhookSecret != null).ToListAsync();

            foreach (var tenant in tenants)
            {
                try
                {
                    var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, tenant.StripeWebhookSecret);
                    await _webhookHandler.ProcessAsync(stripeEvent, tenant.Id);
                    return Ok();
                }
                catch (StripeException)
                {
                    continue;
                }
            }

            return BadRequest("No matching tenant found for webhook signature.");
        }
    }
}
