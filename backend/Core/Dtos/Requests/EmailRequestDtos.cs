namespace Core.Dtos.Requests
{
    public class SendEmailDto
    {
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? PlainTextBody { get; set; }
    }

    public class CreateEmailTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string TemplateKey { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? TextBody { get; set; }
        public string Category { get; set; } = "billing";
        public List<string> Variables { get; set; } = new();
    }

    public class UpdateEmailTemplateDto
    {
        public string? Subject { get; set; }
        public string? HtmlBody { get; set; }
        public string? PlainTextBody { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PreviewEmailTemplateDto
    {
        public Dictionary<string, string> Variables { get; set; } = new();
    }

    public class EmailLogFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Status { get; set; }
        public string? TemplateKey { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
