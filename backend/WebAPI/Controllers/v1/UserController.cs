using Core.Dtos.Requests;
using Core.ServiceContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StripeBilling.API.Controllers.v1
{
    [Route("api/v1/users")]
    [Authorize(Policy = "AdminOrAbove")]
    public class UserController : GatewayControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto request)
        {
            return ToResponse(await _userService.CreateAsync(request));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            return ToResponse(await _userService.GetAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] UserFilterDto filter)
        {
            return ToResponse(await _userService.ListAsync(filter));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto request)
        {
            return ToResponse(await _userService.UpdateAsync(id, request));
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            return ToResponse(await _userService.DeactivateAsync(id));
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            return ToResponse(await _userService.ActivateAsync(id));
        }
    }
}
