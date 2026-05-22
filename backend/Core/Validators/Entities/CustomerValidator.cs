using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class CustomerValidator : EntityBaseValidator<Customer>
    {
        public CustomerValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((customer, context) =>
            {
                if (customer == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Customer"));
                    return;
                }

                if (!IsCurrentTenant(customer.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        }

        public override void AddUpdateRules() { }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((customer, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }
    }
}
