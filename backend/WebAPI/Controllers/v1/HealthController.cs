using Microsoft.AspNetCore.Mvc;
using Core.Dtos.Responses;

namespace StripeBilling.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse> GetHealth()
    {
        var response = ApiResponse.SuccessResponse("API is running");
        return Ok(response);
    }
}
