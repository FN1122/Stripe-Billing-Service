namespace Core.Constants
{
    public static class RuleValidator
    {
        public const string GET = "Get";
        public const string CREATE = "Create";
        public const string UPDATE = "Update";
        public const string DELETE = "Delete";
    }

    public static class ValidationResources
    {
        public static class Fields
        {
            public const string Unauthorized = "Unauthorized";
            public const string TenantId = "TenantId";
            public const string Entity = "Entity";
        }

        public static class Messages
        {
            public const string UnauthorizedAction = "You are not authorized to perform this action.";
            public const string EntityNotFound = "{0} not found.";
            public const string TenantMismatch = "Entity does not belong to the current tenant.";
            public const string EntityInactive = "{0} is not active.";
            public const string InvalidState = "{0} is not in a valid state for this operation.";
            public const string RoleAccessValidation = "Your role ({0}) does not have access to this resource.";
            public const string OwnershipRequired = "Only the owner can perform this action.";
            public const string AdminRequired = "Admin role is required for this action.";
            public const string ManagerOrAboveRequired = "Manager or above role is required for this action.";
        }
    }
}
