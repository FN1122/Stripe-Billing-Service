using Core.Constants;
using Core.ContextProviders;
using Core.Infrastructure;
using FluentValidation;

namespace Core.Validators.Entities
{
    public class UserValidator : EntityBaseValidator<User>
    {
        public UserValidator(ITenantContextProvider tenantContextProvider) : base(tenantContextProvider)
        {
        }

        public override void AddDefaultRules()
        {
            RuleFor(x => x).Custom((user, context) =>
            {
                if (user == null)
                {
                    context.AddFailure(ValidationResources.Fields.Entity, string.Format(ValidationResources.Messages.EntityNotFound, "User"));
                    return;
                }

                if (!IsCurrentTenant(user.TenantId))
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.TenantMismatch);
                }
            });
        }

        public override void AddGetRules() { }

        public override void AddCreateRules()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x).Custom((user, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }

        public override void AddUpdateRules()
        {
            RuleFor(x => x).Custom((user, context) =>
            {
                // Users can update themselves, admins can update anyone
                if (!IsCurrentUser(user.Id) && !IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.OwnershipRequired);
                }
            });
        }

        public override void AddDeleteRules()
        {
            RuleFor(x => x).Custom((user, context) =>
            {
                if (!IsAdmin())
                {
                    context.AddFailure(ValidationResources.Fields.Unauthorized, ValidationResources.Messages.AdminRequired);
                }
            });
        }
    }
}
