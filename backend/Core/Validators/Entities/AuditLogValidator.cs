using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class AuditLogValidator : EntityBaseValidator<AuditLog>
    {
        public AuditLogValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((log, context) =>
            {
                if (log == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "Audit Log"));
                    return;
                }

                if (log.TenantId.HasValue && !IsCurrentTenant(log.TenantId.Value))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules()
        {
            RuleFor(x => x).Custom((log, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddCreateRules() { }
        public override void AddUpdateRules() { }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((log, context) =>
            {
                context.AddFailure(ValidationResources.Fields.Unauthorized, "Audit logs cannot be deleted.");
            });
        }
    }
}
