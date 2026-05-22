namespace Core.Constants;

public static class AppConstants
{
    public const string ApiVersion = "v1";
    public const string ApiBaseUrl = "/api/v1";
    
    public static class ErrorMessages
    {
        public const string InvalidCredentials = "Invalid username or password";
        public const string UserNotFound = "User not found";
        public const string UnauthorizedAccess = "Unauthorized access";
        public const string ValidationFailed = "Validation failed";
    }

    public static class SuccessMessages
    {
        public const string OperationSuccessful = "Operation completed successfully";
        public const string DataRetrieved = "Data retrieved successfully";
    }
}
