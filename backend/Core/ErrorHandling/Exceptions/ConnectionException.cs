namespace Core.ErrorHandling.Exceptions
{
    public class ConnectionException : Exception
    {
        public string ServiceType { get; }
        public ConnectionException(string serviceType, string message) : base(message) { ServiceType = serviceType; }
    }
}
