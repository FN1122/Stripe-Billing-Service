using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/audit-logs")]
    [Authorize(Policy = "AdminOrAbove")]
    public class AuditController : GatewayControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] AuditLogFilterDto filter)
        {
            return ToResponse(await _auditService.ListAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _auditService.GetAsync(id));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            return ToResponse(await _auditService.GetStatsAsync());
        }
    }
}
