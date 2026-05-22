using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/exports")]
    public class ExportController : GatewayControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> ExportTransactions([FromQuery] ExportRequestDto request)
        {
            var bytes = await _exportService.ExportTransactionsAsync(request);
            return File(bytes, request.Format == "pdf" ? "application/pdf" : "text/csv", $"transactions.{request.Format}");
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> ExportInvoices([FromQuery] ExportRequestDto request)
        {
            var bytes = await _exportService.ExportInvoicesAsync(request);
            return File(bytes, request.Format == "pdf" ? "application/pdf" : "text/csv", $"invoices.{request.Format}");
        }

        [HttpGet("customers")]
        public async Task<IActionResult> ExportCustomers([FromQuery] ExportRequestDto request)
        {
            var bytes = await _exportService.ExportCustomersAsync(request);
            return File(bytes, request.Format == "pdf" ? "application/pdf" : "text/csv", $"customers.{request.Format}");
        }

        [HttpGet("subscriptions")]
        public async Task<IActionResult> ExportSubscriptions([FromQuery] ExportRequestDto request)
        {
            var bytes = await _exportService.ExportSubscriptionsAsync(request);
            return File(bytes, request.Format == "pdf" ? "application/pdf" : "text/csv", $"subscriptions.{request.Format}");
        }

        [HttpGet("refunds")]
        public async Task<IActionResult> ExportRefunds([FromQuery] ExportRequestDto request)
        {
            var bytes = await _exportService.ExportRefundsAsync(request);
            return File(bytes, request.Format == "pdf" ? "application/pdf" : "text/csv", $"refunds.{request.Format}");
        }

        [HttpGet("audit-log")]
        public async Task<IActionResult> ExportAuditLog([FromQuery] ExportRequestDto request)
        {
            var bytes = await _exportService.ExportAuditLogAsync(request);
            return File(bytes, request.Format == "pdf" ? "application/pdf" : "text/csv", $"audit-log.{request.Format}");
        }

        [HttpGet("reports/revenue")]
        public async Task<IActionResult> GenerateRevenueReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var bytes = await _exportService.GenerateRevenueReportAsync(from, to);
            return File(bytes, "application/pdf", "revenue-report.pdf");
        }

        [HttpGet("reports/tax")]
        public async Task<IActionResult> GenerateTaxReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var bytes = await _exportService.GenerateTaxReportAsync(from, to);
            return File(bytes, "application/pdf", "tax-report.pdf");
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            return ToResponse(await _exportService.GetExportHistoryAsync());
        }

        [HttpPost("schedule")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> ScheduleReport([FromBody] ScheduleReportDto request)
        {
            return ToResponse(await _exportService.ScheduleReportAsync(request));
        }
    }
}
