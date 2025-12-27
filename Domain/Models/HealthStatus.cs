namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents the health status of the service.
/// </summary>
/// <param name="Status">Current health status (e.g., "Healthy", "Degraded", "Unhealthy").</param>
/// <param name="TimestampUtc">UTC timestamp when the health check was performed.</param>
public record HealthStatus(string Status, DateTime TimestampUtc);
