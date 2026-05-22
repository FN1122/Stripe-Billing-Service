using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class WebhookEventInboundValidator : EntityBaseValidator<WebhookEventInbound>
    {
        public WebhookEventInboundValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((evt, context) =>
            {
                if (evt == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Webhook Event"));
                    return;
                }

                if (!IsCurrentTenant(evt.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }
        public override void AddCreateRules() { }
        public override void AddUpdateRules() { }
        public override void AddDeleteRules() { }
    }
}
