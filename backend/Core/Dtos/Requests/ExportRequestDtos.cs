namespace Core.Dtos.Requests
{
    public class ExportRequestDto
    {
        public string Format { get; set; } = "csv"; // csv | pdf
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ScheduleReportDto
    {
        public string ReportType { get; set; } = "revenue"; // revenue | tax | transactions
        public string Schedule { get; set; } = "monthly"; // daily | weekly | monthly
        public string Format { get; set; } = "pdf";
        public string? Email { get; set; }
    }
}
