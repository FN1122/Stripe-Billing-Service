using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class RefundValidator : EntityBaseValidator<Refund>
    {
        public RefundValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((refund, context) =>
            {
                if (refund == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Refund"));
                    return;
                }

                if (!IsCurrentTenant(refund.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.TransactionId).NotEmpty();
            RuleFor(x => x).Custom((refund, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((refund, context) =>
            {
                if (refund.Status == "processed" || refund.Status == "rejected")
                {
                    context.AddFailure(nameof(refund.Status), string.Format(ValidationResources.Messages.InvalidState, "Refund"));
                }

                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((refund, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }
    }
}
