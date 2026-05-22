namespace Core.ErrorHandling.Exceptions
{
    public class FeatureNotAvailableException : Exception
    {
        public string FeatureName { get; }
        public FeatureNotAvailableException(string featureName) : base($"Feature '{featureName}' is not available.") { FeatureName = featureName; }
    }
}
