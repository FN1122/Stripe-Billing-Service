namespace Core.Dtos.Requests
{
    public class InvoiceFilterDto
    {
        public string? Status { get; set; }
        public string? CustomerSearch { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
