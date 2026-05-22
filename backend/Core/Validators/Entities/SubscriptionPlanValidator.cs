using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class SubscriptionPlanValidator : EntityBaseValidator<SubscriptionPlan>
    {
        public SubscriptionPlanValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((plan, context) =>
            {
                if (plan == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Subscription Plan"));
                    return;
                }

                if (!IsCurrentTenant(plan.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Interval).NotEmpty().Must(x => new[] { "month", "year", "week", "day" }.Contains(x))
                .WithMessage("Interval must be month, year, week, or day.");
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((plan, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((plan, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }
    }
}
