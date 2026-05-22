using Core.Dtos.Requests;
using Core.Dtos.Responses;
using Core.Utils;

namespace Core.ServiceContracts
{
    public interface IExportService
    {
        Task<byte[]> ExportTransactionsAsync(ExportRequestDto request);
        Task<byte[]> ExportInvoicesAsync(ExportRequestDto request);
        Task<byte[]> ExportCustomersAsync(ExportRequestDto request);
        Task<byte[]> ExportSubscriptionsAsync(ExportRequestDto request);
        Task<byte[]> ExportRefundsAsync(ExportRequestDto request);
        Task<byte[]> ExportAuditLogAsync(ExportRequestDto request);
        Task<byte[]> GenerateRevenueReportAsync(DateTime from, DateTime to);
        Task<byte[]> GenerateTaxReportAsync(DateTime from, DateTime to);
        Task<GatewayResponseWrapper<List<ExportLogDto>>> GetExportHistoryAsync();
        Task<GatewayResponseWrapper<bool>> ScheduleReportAsync(ScheduleReportDto request);
    }
}
