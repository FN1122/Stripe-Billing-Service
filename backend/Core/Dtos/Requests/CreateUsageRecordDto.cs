namespace Core.Dtos.Requests
{
    public class CreateUsageRecordDto
    {
        public Guid SubscriptionId { get; set; }
        public long Quantity { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Action { get; set; } = "increment"; // increment | set
        public string? IdempotencyKey { get; set; }
    }

    public class BatchUsageRecordDto
    {
        public List<CreateUsageRecordDto> Records { get; set; } = new();
    }

    public class UsageFilterDto
    {
        public Guid? SubscriptionId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class CreateMeterEventDto
    {
        public Guid CustomerId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public long Value { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? Properties { get; set; }
    }
}
