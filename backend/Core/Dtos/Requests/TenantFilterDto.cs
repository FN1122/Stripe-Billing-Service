namespace Core.Dtos.Requests
{
    public class TenantFilterDto
    {
        public string? Search { get; set; }
        public string? Plan { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
