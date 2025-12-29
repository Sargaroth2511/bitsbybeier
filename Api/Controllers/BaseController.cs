using Microsoft.AspNetCore.Mvc;
using bitsbybeier.Data;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Base controller providing common functionality for all API controllers.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Logger instance for the controller.
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Database context for data access.
    /// </summary>
    protected readonly ApplicationDbContext Context;

    /// <summary>
    /// Initializes a new instance of the BaseController.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="context">Database context.</param>
    protected BaseController(ILogger logger, ApplicationDbContext context)
    {
        Logger = logger;
        Context = context;
    }

    /// <summary>
    /// Gets the current user's ID from claims, if available.
    /// </summary>
    protected string? UserId => User?.FindFirst("sub")?.Value ?? User?.FindFirst("id")?.Value;

    /// <summary>
    /// Gets the current user's email from claims, if available.
    /// </summary>
    protected string? UserEmail => User?.FindFirst("email")?.Value;

    /// <summary>
    /// Gets the current user's display name from claims, if available.
    /// </summary>
    protected string? UserDisplayName => User?.FindFirst("name")?.Value;

    /// <summary>
    /// Indicates whether the current user is authenticated.
    /// </summary>
    protected bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
