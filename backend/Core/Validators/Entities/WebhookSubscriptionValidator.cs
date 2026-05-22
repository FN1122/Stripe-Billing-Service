using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class WebhookSubscriptionValidator : EntityBaseValidator<WebhookSubscription>
    {
        public WebhookSubscriptionValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((webhook, context) =>
            {
                if (webhook == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Webhook Subscription"));
                    return;
                }

                if (!IsCurrentTenant(webhook.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x).Custom((webhook, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((webhook, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((webhook, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }
    }
}
