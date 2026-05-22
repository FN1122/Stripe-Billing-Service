using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class SubscriptionValidator : EntityBaseValidator<Subscription>
    {
        public SubscriptionValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((subscription, context) =>
            {
                if (subscription == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Subscription"));
                    return;
                }

                if (!IsCurrentTenant(subscription.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.PlanId).NotEmpty();
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((subscription, context) =>
            {
                if (subscription.Status == "canceled")
                {
                    context.AddFailure(nameof(subscription.Status), string.Format(ValidationResources.Messages.InvalidState, "Subscription"));
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((subscription, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }
    }
}
