namespace Core.Constants
{
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Viewer = "Viewer";

        public const string AdminOrAbove = "SuperAdmin,Admin";
        public const string ManagerOrAbove = "SuperAdmin,Admin,Manager";
        public const string AllRoles = "SuperAdmin,Admin,Manager,Viewer";

        public static bool IsValid(string role) =>
            role is SuperAdmin or Admin or Manager or Viewer;
    }
}
