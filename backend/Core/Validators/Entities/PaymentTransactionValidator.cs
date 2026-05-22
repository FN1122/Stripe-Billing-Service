using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class PaymentTransactionValidator : EntityBaseValidator<PaymentTransaction>
    {
        public PaymentTransactionValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((transaction, context) =>
            {
                if (transaction == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Payment Transaction"));
                    return;
                }

                if (!IsCurrentTenant(transaction.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.Currency).NotEmpty().MaximumLength(3);
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((transaction, context) =>
            {
                if (transaction.Status == "refunded")
                {
                    context.AddFailure(nameof(transaction.Status), string.Format(ValidationResources.Messages.InvalidState, "Payment Transaction"));
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((transaction, context) =>
            {
                context.AddFailure(ValidationResources.Fields.Unauthorized, "Payment transactions cannot be deleted.");
            });
        }
    }
}
