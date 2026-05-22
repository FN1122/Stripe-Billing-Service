using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class TenantValidator : EntityBaseValidator<Tenant>
    {
        public TenantValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((tenant, context) =>
            {
                if (tenant == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Tenant"));
                    return;
                }

                if (!IsCurrentTenant(tenant.Id))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((tenant, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((tenant, context) =>
            {
                if (CurrentTenantContext.Role != Roles.SuperAdmin)
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, "Only SuperAdmin can delete tenants.");
                }
            });
        }
    }
}
