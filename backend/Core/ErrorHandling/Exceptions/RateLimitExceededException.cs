namespace Core.ErrorHandling.Exceptions
{
    public class RateLimitExceededException : Exception
    {
        public int RetryAfterSeconds { get; }
        public RateLimitExceededException(int retryAfterSeconds = 60) : base("Rate limit exceeded.") { RetryAfterSeconds = retryAfterSeconds; }
    }
}
