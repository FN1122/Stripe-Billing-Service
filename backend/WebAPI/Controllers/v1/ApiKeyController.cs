using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/api-keys")]
    [Authorize(Policy = "ManagerOrAbove")]
    public class ApiKeyController : GatewayControllerBase
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyController(IApiKeyService apiKeyService)
        {
            _apiKeyService = apiKeyService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApiKeyDto request)
        {
            return ToResponse(await _apiKeyService.CreateAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _apiKeyService.GetAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] ApiKeyFilterDto filter)
        {
            return ToResponse(await _apiKeyService.ListAsync(filter));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApiKeyDto request)
        {
            return ToResponse(await _apiKeyService.UpdateAsync(id, request));
        }

        [HttpPost("{id}/revoke")]
        public async Task<IActionResult> Revoke(Guid id)
        {
            return ToResponse(await _apiKeyService.RevokeAsync(id));
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            return ToResponse(await _apiKeyService.RestoreAsync(id));
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            return ToResponse(await _apiKeyService.GetStatsAsync());
        }
    }
}
