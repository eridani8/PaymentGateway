using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HealthController : ControllerBase
{
    public IActionResult CheckAvailable()
    {
        return Ok();
    }
}