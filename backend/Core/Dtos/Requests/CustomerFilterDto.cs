namespace Core.Dtos.Requests
{
    public class CustomerFilterDto
    {
        public string? Search { get; set; }
        public bool? HasSubscription { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
