using bitsbybeier.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace bitsbybeier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthStatus> Get()
    {
        var status = new HealthStatus("Healthy", DateTime.UtcNow);
        return Ok(status);
    }
}
