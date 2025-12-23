using bitsbybeier.Api.Controllers;
using bitsbybeier.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace bitsbybeier.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsHealthyStatus()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var healthStatus = Assert.IsType<HealthStatus>(okResult.Value);
        Assert.Equal("Healthy", healthStatus.Status);
        Assert.True((DateTime.UtcNow - healthStatus.TimestampUtc).TotalSeconds < 1);
    }
}
