using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult CheckAvailable()
    {
        return Ok();
    }
}