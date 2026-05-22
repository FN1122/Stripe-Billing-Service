namespace Core.ErrorHandling.Exceptions
{
    public class ServiceUnavailableException : Exception
    {
        public string ServiceName { get; }
        public ServiceUnavailableException(string serviceName) : base($"Service '{serviceName}' is currently unavailable.") { ServiceName = serviceName; }
    }
}
