using Core.Constants;
using Core.ContextProviders;
using FluentValidation;

namespace Core.Validators
{
    public abstract class EntityBaseValidator<T> : AbstractValidator<T>
    {
        private readonly ITenantContextProvider _tenantContextProvider;
        protected TenantContext CurrentTenantContext => _tenantContextProvider.GetCurrentTenantContext();

        public EntityBaseValidator(ITenantContextProvider tenantContextProvider)
        {
            _tenantContextProvider = tenantContextProvider;

            RuleSet(RuleValidator.GET, () =>
            {
                AddDefaultRules();
                AddGetRules();
            });

            RuleSet(RuleValidator.CREATE, () =>
            {
                AddDefaultRules();
                AddCreateRules();
            });

            RuleSet(RuleValidator.UPDATE, () =>
            {
                AddDefaultRules();
                AddUpdateRules();
            });

            RuleSet(RuleValidator.DELETE, () =>
            {
                AddDefaultRules();
                AddDeleteRules();
            });
        }

        public abstract void AddDefaultRules();
        public abstract void AddGetRules();
        public abstract void AddCreateRules();
        public abstract void AddUpdateRules();
        public abstract void AddDeleteRules();

        #region Helper Methods

        protected bool IsCurrentTenant(Guid tenantId)
        {
            return tenantId == CurrentTenantContext.TenantId;
        }

        protected bool IsCurrentUser(Guid userId)
        {
            return userId == CurrentTenantContext.UserId;
        }

        protected bool IsAdmin()
        {
            return CurrentTenantContext.Role == Roles.Admin || CurrentTenantContext.Role == Roles.SuperAdmin;
        }

        protected bool IsManagerOrAbove()
        {
            return CurrentTenantContext.Role == Roles.SuperAdmin
                || CurrentTenantContext.Role == Roles.Admin
                || CurrentTenantContext.Role == Roles.Manager;
        }

        protected string ValidateRoleAccess(string requiredRole)
        {
            return CurrentTenantContext.Role != requiredRole
                ? string.Format(ValidationResources.Messages.RoleAccessValidation, requiredRole)
                : null;
        }

        #endregion
    }
}
