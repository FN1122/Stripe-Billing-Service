namespace Core.Dtos.Requests
{
    public class DunningConfigDto
    {
        public List<DunningStepConfigDto> Steps { get; set; } = new();
        public int GracePeriodDays { get; set; } = 3;
        public int MaxRetryAttempts { get; set; } = 4;
        public bool AutoCancelAfterMaxRetries { get; set; } = true;
    }

    public class DunningStepConfigDto
    {
        public int DaysAfterFailure { get; set; }
        public string Action { get; set; } = "retry_payment";
        public string? EmailTemplateKey { get; set; }
    }

    public class DunningFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; }
        public Guid? CustomerId { get; set; }
    }
}
