using Core.ContextProviders;
using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Infrastructure;
using Core.RepositoryContracts;
using Core.ServiceContracts;
using Core.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Core.Services
{
    public class EmailService : BaseService, IEmailService
    {
        private readonly IEmailRepository _emailRepo;

        public EmailService(ITenantContextProvider tenantContextProvider, IEmailRepository emailRepo) : base(tenantContextProvider)
        {
            _emailRepo = emailRepo;
        }

        public async Task<GatewayResponseWrapper<EmailLogResponseDto>> SendAsync(SendEmailDto request)
        {
            var response = new GatewayResponseWrapper<EmailLogResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var log = new EmailLog
            {
                TenantId = tenantId,
                To = request.To,
                Cc = request.Cc,
                Bcc = request.Bcc,
                Subject = request.Subject,
                Status = "queued"
            };

            await _emailRepo.CreateLogAsync(log);
            // In production, dispatch to email provider here
            log.Status = "sent";
            log.SentAt = DateTime.UtcNow;
            await _emailRepo.UpdateLogAsync(log);

            response.SetSuccess(MapLog(log));
            return response;
        }

        public async Task<GatewayResponseWrapper<EmailLogResponseDto>> SendTemplatedAsync(string templateKey, string to, Dictionary<string, string> variables)
        {
            var response = new GatewayResponseWrapper<EmailLogResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var template = await _emailRepo.GetTemplateAsync(tenantId, templateKey);
            if (template == null) { response.SetError("Template not found.", 404); return response; }

            var subject = ReplaceVariables(template.Subject, variables);
            var body = ReplaceVariables(template.HtmlBody, variables);

            return await SendAsync(new SendEmailDto { To = to, Subject = subject, HtmlBody = body });
        }

        public async Task<GatewayResponseWrapper<List<EmailTemplateResponseDto>>> GetTemplatesAsync()
        {
            var response = new GatewayResponseWrapper<List<EmailTemplateResponseDto>>();
            var tenantId = CurrentTenantContext.TenantId;
            var templates = await _emailRepo.GetTemplatesAsync(tenantId);
            response.SetSuccess(templates.Select(MapTemplate).ToList());
            return response;
        }

        public async Task<GatewayResponseWrapper<EmailTemplateResponseDto>> GetTemplateAsync(string templateKey)
        {
            var response = new GatewayResponseWrapper<EmailTemplateResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var template = await _emailRepo.GetTemplateAsync(tenantId, templateKey);
            if (template == null) { response.SetError("Template not found.", 404); return response; }
            response.SetSuccess(MapTemplate(template));
            return response;
        }

        public async Task<GatewayResponseWrapper<EmailTemplateResponseDto>> CreateTemplateAsync(CreateEmailTemplateDto request)
        {
            var response = new GatewayResponseWrapper<EmailTemplateResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var templateKey = request.Name.ToLowerInvariant().Replace(" ", "_");
            var existing = await _emailRepo.GetTemplateAsync(tenantId, templateKey);
            if (existing != null)
            {
                response.SetError("A template with this name already exists.");
                return response;
            }

            var template = new EmailTemplate
            {
                TenantId = tenantId,
                TemplateKey = templateKey,
                Subject = request.Subject,
                HtmlBody = request.HtmlBody,
                PlainTextBody = request.TextBody,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _emailRepo.CreateTemplateAsync(template);
            response.SetSuccess(MapTemplate(template));
            return response;
        }

        public async Task<GatewayResponseWrapper<EmailTemplateResponseDto>> UpdateTemplateAsync(string templateKey, UpdateEmailTemplateDto request)
        {
            var response = new GatewayResponseWrapper<EmailTemplateResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var template = await _emailRepo.GetTemplateAsync(tenantId, templateKey);
            if (template == null) { response.SetError("Template not found.", 404); return response; }

            if (request.Subject != null) template.Subject = request.Subject;
            if (request.HtmlBody != null) template.HtmlBody = request.HtmlBody;
            if (request.PlainTextBody != null) template.PlainTextBody = request.PlainTextBody;
            if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;
            template.UpdatedAt = DateTime.UtcNow;

            await _emailRepo.UpdateTemplateAsync(template);
            response.SetSuccess(MapTemplate(template));
            return response;
        }

        public async Task<GatewayResponseWrapper<EmailTemplateResponseDto>> ResetTemplateAsync(string templateKey)
        {
            var response = new GatewayResponseWrapper<EmailTemplateResponseDto>();
            response.SetError("Template reset to defaults is not yet implemented.", 501);
            return response;
        }

        public async Task<GatewayResponseWrapper<string>> PreviewTemplateAsync(string templateKey, Dictionary<string, string> variables)
        {
            var response = new GatewayResponseWrapper<string>();
            var tenantId = CurrentTenantContext.TenantId;
            var template = await _emailRepo.GetTemplateAsync(tenantId, templateKey);
            if (template == null) { response.SetError("Template not found.", 404); return response; }

            var html = ReplaceVariables(template.HtmlBody, variables);
            response.SetSuccess(html);
            return response;
        }

        public async Task<GatewayPaginatedListResponseWrapper<EmailLogResponseDto>> GetEmailLogsAsync(EmailLogFilterDto filter)
        {
            var response = new GatewayPaginatedListResponseWrapper<EmailLogResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var query = _emailRepo.QueryLogs(tenantId);

            if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(l => l.Status == filter.Status);
            if (!string.IsNullOrEmpty(filter.TemplateKey)) query = query.Where(l => l.TemplateKey == filter.TemplateKey);
            if (!string.IsNullOrEmpty(filter.Search)) query = query.Where(l => l.To.Contains(filter.Search) || l.Subject.Contains(filter.Search));
            if (filter.FromDate.HasValue) query = query.Where(l => l.CreatedAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue) query = query.Where(l => l.CreatedAt <= filter.ToDate.Value);

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(l => l.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();

            response.SetSuccessWithPagination(items.Select(MapLog).ToList(), totalCount, filter.Page, filter.PageSize);
            return response;
        }

        public async Task<GatewayResponseWrapper<EmailLogResponseDto>> ResendEmailAsync(Guid emailLogId)
        {
            var response = new GatewayResponseWrapper<EmailLogResponseDto>();
            var tenantId = CurrentTenantContext.TenantId;
            var log = await _emailRepo.GetEmailLogAsync(tenantId, emailLogId);
            if (log == null) { response.SetError("Email log not found.", 404); return response; }

            log.Status = "queued";
            log.ErrorMessage = null;
            await _emailRepo.UpdateLogAsync(log);
            response.SetSuccess(MapLog(log), "Email queued for resend.");
            return response;
        }

        public async Task<GatewayResponseWrapper<EmailStatsDto>> GetStatsAsync()
        {
            var response = new GatewayResponseWrapper<EmailStatsDto>();
            var tenantId = CurrentTenantContext.TenantId;

            var stats = new EmailStatsDto
            {
                TotalSent = await _emailRepo.CountByStatusAsync(tenantId, "sent"),
                TotalDelivered = await _emailRepo.CountByStatusAsync(tenantId, "delivered"),
                TotalFailed = await _emailRepo.CountByStatusAsync(tenantId, "failed"),
                TotalBounced = await _emailRepo.CountByStatusAsync(tenantId, "bounced"),
            };
            var total = stats.TotalSent + stats.TotalDelivered + stats.TotalFailed + stats.TotalBounced;
            stats.DeliveryRate = total > 0 ? Math.Round((decimal)(stats.TotalSent + stats.TotalDelivered) / total * 100, 2) : 0;

            response.SetSuccess(stats);
            return response;
        }

        private string ReplaceVariables(string template, Dictionary<string, string> variables)
        {
            foreach (var kvp in variables)
                template = template.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            return template;
        }

        private EmailTemplateResponseDto MapTemplate(EmailTemplate t)
        {
            var variables = new List<string>();
            if (!string.IsNullOrEmpty(t.Variables))
            {
                try { variables = JsonConvert.DeserializeObject<List<string>>(t.Variables) ?? new(); } catch { }
            }
            return new EmailTemplateResponseDto
            {
                Id = t.Id, TemplateKey = t.TemplateKey, Subject = t.Subject,
                HtmlBody = t.HtmlBody, PlainTextBody = t.PlainTextBody,
                IsActive = t.IsActive, Variables = variables,
                CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
            };
        }

        private EmailLogResponseDto MapLog(EmailLog l) => new()
        {
            Id = l.Id, TemplateKey = l.TemplateKey, To = l.To, Subject = l.Subject,
            Status = l.Status, Provider = l.Provider, ErrorMessage = l.ErrorMessage,
            SentAt = l.SentAt, DeliveredAt = l.DeliveredAt, CreatedAt = l.CreatedAt
        };
    }
}
