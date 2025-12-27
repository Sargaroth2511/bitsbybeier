using bitsbybeier.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for health check and service status monitoring.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Gets the health status of the service.
    /// </summary>
    /// <returns>Current health status with timestamp.</returns>
    /// <response code="200">Service is healthy and operational.</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    public ActionResult<HealthStatus> Get()
    {
        var status = new HealthStatus("Healthy", DateTime.UtcNow);
        return Ok(status);
    }
}
