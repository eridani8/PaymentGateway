using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]/[action]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> CheckAvailable()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}