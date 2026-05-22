using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class InvoiceValidator : EntityBaseValidator<Invoice>
    {
        public InvoiceValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((invoice, context) =>
            {
                if (invoice == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Invoice"));
                    return;
                }

                if (!IsCurrentTenant(invoice.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.Total).GreaterThanOrEqualTo(0);
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((invoice, context) =>
            {
                if (invoice.Status == "paid" || invoice.Status == "void")
                {
                    context.AddFailure(nameof(invoice.Status), string.Format(ValidationResources.Messages.InvalidState, "Invoice"));
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((invoice, context) =>
            {
                context.AddFailure(ValidationResources.Fields.Unauthorized, "Invoices cannot be deleted.");
            });
        }
    }
}
