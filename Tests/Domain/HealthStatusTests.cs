using bitsbybeier.Domain.Models;

namespace bitsbybeier.Tests.Domain;

public class HealthStatusTests
{
    [Fact]
    public void HealthStatus_CanBeCreated()
    {
        // Arrange
        var status = "Healthy";
        var timestamp = DateTime.UtcNow;

        // Act
        var healthStatus = new HealthStatus(status, timestamp);

        // Assert
        Assert.Equal(status, healthStatus.Status);
        Assert.Equal(timestamp, healthStatus.TimestampUtc);
    }

    [Fact]
    public void HealthStatus_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var healthStatus1 = new HealthStatus("Healthy", timestamp);
        var healthStatus2 = new HealthStatus("Healthy", timestamp);
        var healthStatus3 = new HealthStatus("Unhealthy", timestamp);

        // Assert
        Assert.Equal(healthStatus1, healthStatus2);
        Assert.NotEqual(healthStatus1, healthStatus3);
    }
}
