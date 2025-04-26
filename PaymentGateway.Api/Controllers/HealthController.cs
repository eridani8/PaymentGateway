using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> CheckAvailable()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}