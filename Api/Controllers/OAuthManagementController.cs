using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Domain.Models;
using bitsbybeier.Data;
using bitsbybeier.Api.Services;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Admin endpoints for managing OAuth clients.
/// </summary>
[Route("api/oauth/admin")]
[Authorize(Roles = "Admin")]
[ApiController]
public class OAuthManagementController : BaseController
{
    public OAuthManagementController(
        ILogger<OAuthManagementController> logger,
        ApplicationDbContext context)
        : base(logger, context)
    {
    }
    
    /// <summary>
    /// Creates a new OAuth client for connecting external applications.
    /// </summary>
    [HttpPost("clients")]
    public async Task<IActionResult> CreateClient([FromBody] CreateOAuthClientRequest request)
    {
        var clientId = Guid.NewGuid().ToString("N");
        var clientSecret = GenerateClientSecret();
        
        var client = new OAuthClient
        {
            ClientId = clientId,
            ClientSecret = OAuthService.HashClientSecret(clientSecret), // Store hashed
            ClientName = request.ClientName,
            RedirectUris = JsonSerializer.Serialize(request.RedirectUris),
            AllowedScopes = JsonSerializer.Serialize(request.AllowedScopes ?? new List<string> { "mcp:read", "mcp:write" }),
            Active = true,
            CreatedByUserId = int.Parse(UserId ?? "0")
        };
        
        Context.OAuthClients.Add(client);
        await Context.SaveChangesAsync();
        
        Logger.LogInformation("OAuth client created: {ClientId} by user {UserId}", clientId, UserId);
        
        // Return client secret ONLY once, it won't be retrievable later
        return Ok(new
        {
            clientId = clientId,
            clientSecret = clientSecret, // Plain text, shown only once
            clientName = client.ClientName,
            redirectUris = request.RedirectUris,
            allowedScopes = request.AllowedScopes,
            message = "Save the client secret securely. It cannot be retrieved later."
        });
    }
    
    /// <summary>
    /// Lists all OAuth clients (without secrets).
    /// </summary>
    [HttpGet("clients")]
    public async Task<IActionResult> ListClients()
    {
        var clients = await Context.OAuthClients
            .Where(c => c.Active)
            .Select(c => new
            {
                c.ClientId,
                c.ClientName,
                RedirectUris = JsonSerializer.Deserialize<List<string>>(c.RedirectUris, (JsonSerializerOptions?)null),
                AllowedScopes = JsonSerializer.Deserialize<List<string>>(c.AllowedScopes, (JsonSerializerOptions?)null),
                c.CreatedAt
            })
            .ToListAsync();
        
        return Ok(clients);
    }
    
    /// <summary>
    /// Deactivates an OAuth client.
    /// </summary>
    [HttpDelete("clients/{clientId}")]
    public async Task<IActionResult> DeactivateClient(string clientId)
    {
        var client = await Context.OAuthClients
            .FirstOrDefaultAsync(c => c.ClientId == clientId);
        
        if (client == null)
        {
            return NotFound();
        }
        
        client.Active = false;
        await Context.SaveChangesAsync();
        
        Logger.LogInformation("OAuth client deactivated: {ClientId} by user {UserId}", clientId, UserId);
        
        return Ok();
    }
    
    private static string GenerateClientSecret()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

public record CreateOAuthClientRequest(
    string ClientName,
    List<string> RedirectUris,
    List<string>? AllowedScopes);
