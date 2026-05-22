using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories
{
    public class WebhookEventInboundRepository : BaseRepository, IWebhookEventInboundRepository
    {
        private readonly BillingDbContext _dbContext;
        private readonly IValidator<WebhookEventInbound> _validator;

        public WebhookEventInboundRepository(ITenantContextProvider tenantContextProvider, BillingDbContext dbContext, IValidator<WebhookEventInbound> validator) : base(tenantContextProvider)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<WebhookEventInbound?> GetByStripeEventIdAsync(Guid tenantId, string stripeEventId)
        {
            return await _dbContext.WebhookEventsInbound.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.StripeEventId == stripeEventId);
        }

        public async Task<Guid> CreateAsync(WebhookEventInbound webhookEvent)
        {
            await _validator.ValidateAndThrowAsync(webhookEvent, RuleValidator.CREATE);
            _dbContext.WebhookEventsInbound.Add(webhookEvent);
            await _dbContext.SaveChangesAsync();
            return webhookEvent.Id;
        }

        public async Task UpdateAsync(WebhookEventInbound webhookEvent)
        {
            await _validator.ValidateAndThrowAsync(webhookEvent, RuleValidator.UPDATE);
            _dbContext.WebhookEventsInbound.Update(webhookEvent);
            await _dbContext.SaveChangesAsync();
        }
    }
}
