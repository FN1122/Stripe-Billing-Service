using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class ApiKeyValidator : EntityBaseValidator<ApiKey>
    {
        public ApiKeyValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((apiKey, context) =>
            {
                if (apiKey == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "API Key"));
                    return;
                }

                if (!IsCurrentTenant(apiKey.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x).Custom((apiKey, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((apiKey, context) =>
            {
                if (!IsManagerOrAbove())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.ManagerOrAboveRequired);
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((apiKey, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }
    }
}
