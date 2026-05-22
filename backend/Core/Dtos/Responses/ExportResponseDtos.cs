namespace Core.Dtos.Responses
{
    public class ExportLogDto
    {
        public Guid Id { get; set; }
        public string ExportType { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public long? FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
