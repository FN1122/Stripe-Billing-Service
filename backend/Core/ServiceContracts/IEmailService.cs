using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IEmailService
    {
        Task<GatewayResponseWrapper<EmailLogResponseDto>> SendAsync(SendEmailDto request);
        Task<GatewayResponseWrapper<EmailLogResponseDto>> SendTemplatedAsync(string templateKey, string to, Dictionary<string, string> variables);
        Task<GatewayResponseWrapper<List<EmailTemplateResponseDto>>> GetTemplatesAsync();
        Task<GatewayResponseWrapper<EmailTemplateResponseDto>> GetTemplateAsync(string templateKey);
        Task<GatewayResponseWrapper<EmailTemplateResponseDto>> CreateTemplateAsync(CreateEmailTemplateDto request);
        Task<GatewayResponseWrapper<EmailTemplateResponseDto>> UpdateTemplateAsync(string templateKey, UpdateEmailTemplateDto request);
        Task<GatewayResponseWrapper<EmailTemplateResponseDto>> ResetTemplateAsync(string templateKey);
        Task<GatewayResponseWrapper<string>> PreviewTemplateAsync(string templateKey, Dictionary<string, string> variables);
        Task<GatewayPaginatedListResponseWrapper<EmailLogResponseDto>> GetEmailLogsAsync(EmailLogFilterDto filter);
        Task<GatewayResponseWrapper<EmailLogResponseDto>> ResendEmailAsync(Guid emailLogId);
        Task<GatewayResponseWrapper<EmailStatsDto>> GetStatsAsync();
    }
}
