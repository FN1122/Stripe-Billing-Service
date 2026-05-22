namespace Core.ErrorHandling.Exceptions;

public class AppException : Exception
{
    public int? ErrorCode { get; set; }
    public string? ErrorDetails { get; set; }

    public AppException(string message) : base(message) { }

    public AppException(string message, int errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public AppException(string message, Exception innerException) 
        : base(message, innerException) { }

    public AppException(string message, int errorCode, string errorDetails) 
        : base(message)
    {
        ErrorCode = errorCode;
        ErrorDetails = errorDetails;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ValidationException : AppException
{
    public ValidationException(string message) : base(message) { }
}

public class StripeException : AppException
{
    public StripeException(string message) : base(message) { }
    public StripeException(string message, Exception innerException) 
        : base(message, innerException) { }
}
