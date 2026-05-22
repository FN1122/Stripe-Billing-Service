namespace Core.Dtos.Requests
{
    public class CouponFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Type { get; set; } // percent_off | amount_off
        public string? Duration { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
