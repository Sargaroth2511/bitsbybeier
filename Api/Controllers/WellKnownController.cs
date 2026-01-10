using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for .well-known endpoints (RFC 8615).
/// </summary>
[Route(".well-known")]
[ApiController]
[AllowAnonymous]
public class WellKnownController : ControllerBase
{
    /// <summary>
    /// OAuth 2.0 Protected Resource Metadata endpoint (RFC 9728).
    /// Used by MCP clients like ChatGPT Desktop to discover authorization servers.
    /// </summary>
    [HttpGet("oauth-protected-resource/mcp")]
    public IActionResult GetOAuthProtectedResourceMcp()
    {
        Console.WriteLine("[DCR] OAuth Protected Resource endpoint called!");
        var scheme = Request.Host.Host == "bitsbybeier.de" ? "https" : Request.Scheme;
        var baseUrl = $"{scheme}://{Request.Host}";
        
        return Ok(new
        {
            resource = $"{baseUrl}/api/mcp",
            authorization_servers = new[] { baseUrl }
        });
    }
}
