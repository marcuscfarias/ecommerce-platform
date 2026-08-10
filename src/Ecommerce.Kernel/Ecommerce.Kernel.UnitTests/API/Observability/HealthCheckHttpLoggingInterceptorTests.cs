using Ecommerce.Kernel.API.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;

namespace Ecommerce.Kernel.UnitTests.API.Observability;

public class HealthCheckHttpLoggingInterceptorTests
{
    private readonly HealthCheckHttpLoggingInterceptor _interceptor = new();

    [Theory]
    [InlineData("/health")] // the mapped probe endpoint
    [InlineData("/health/ready")] // any sub-path under it
    public async Task OnRequestAsync_HealthCheckPath_ShouldDisableLogging(string path)
    {
        // Arrange
        var context = CreateContext(path);

        // Act
        await _interceptor.OnRequestAsync(context);

        // Assert
        context.LoggingFields.ShouldBe(HttpLoggingFields.None);
    }

    [Theory]
    [InlineData("/products")] // regular traffic
    [InlineData("/healthy-snacks")] // shares the prefix but is not a path segment
    public async Task OnRequestAsync_OtherPath_ShouldKeepLogging(string path)
    {
        // Arrange
        var context = CreateContext(path);

        // Act
        await _interceptor.OnRequestAsync(context);

        // Assert
        context.LoggingFields.ShouldBe(HttpLoggingFields.All);
    }

    private static HttpLoggingInterceptorContext CreateContext(string path)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = path;

        return new HttpLoggingInterceptorContext
        {
            HttpContext = httpContext,
            LoggingFields = HttpLoggingFields.All,
        };
    }
}
