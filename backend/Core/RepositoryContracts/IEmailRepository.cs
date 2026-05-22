using Core.Infrastructure;

namespace Core.RepositoryContracts
{
    public interface IEmailRepository
    {
        Task<List<EmailTemplate>> GetTemplatesAsync(Guid tenantId);
        Task<EmailTemplate?> GetTemplateAsync(Guid tenantId, string templateKey);
        Task<Guid> CreateTemplateAsync(EmailTemplate template);
        Task UpdateTemplateAsync(EmailTemplate template);
        Task<EmailLog?> GetEmailLogAsync(Guid tenantId, Guid id);
        IQueryable<EmailLog> QueryLogs(Guid tenantId);
        Task<Guid> CreateLogAsync(EmailLog log);
        Task UpdateLogAsync(EmailLog log);
        Task<int> CountByStatusAsync(Guid tenantId, string status);
    }
}
