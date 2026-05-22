using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/customers")]
    public class CustomerController : GatewayControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto request)
        {
            return ToResponse(await _customerService.CreateAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _customerService.GetAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto request)
        {
            return ToResponse(await _customerService.UpdateAsync(id, request));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] CustomerFilterDto filter)
        {
            return ToResponse(await _customerService.ListAsync(filter));
        }

        [HttpPost("{id}/portal-session")]
        public async Task<IActionResult> CreatePortalSession(Guid id)
        {
            return ToResponse(await _customerService.CreatePortalSessionAsync(id));
        }
    }
}
