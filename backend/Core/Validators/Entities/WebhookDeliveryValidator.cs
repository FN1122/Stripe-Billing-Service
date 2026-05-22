using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class WebhookDeliveryValidator : EntityBaseValidator<WebhookDelivery>
    {
        public WebhookDeliveryValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((delivery, context) =>
            {
                if (delivery == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Webhook Delivery"));
                    return;
                }

                if (!IsCurrentTenant(delivery.TenantId))
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
