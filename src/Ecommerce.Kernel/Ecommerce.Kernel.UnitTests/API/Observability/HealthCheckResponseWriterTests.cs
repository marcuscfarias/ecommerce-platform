using System.Text.Json;
using Ecommerce.Kernel.API.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ecommerce.Kernel.UnitTests.API.Observability;

public class HealthCheckResponseWriterTests
{
    private readonly Faker _faker = new();

    [Fact]
    public async Task WriteAsync_DegradedEntry_ShouldWriteStatusAndEntries()
    {
        // Arrange
        var description = _faker.Lorem.Sentence();
        var report = CreateReport(new HealthReportEntry(
            HealthStatus.Degraded, description, TimeSpan.Zero, exception: null, data: null));
        var context = CreateContext();

        // Act
        await HealthCheckResponseWriter.WriteAsync(context, report);

        // Assert
        context.Response.ContentType.ShouldStartWith("application/json");
        using var body = await ReadBodyAsync(context);
        body.RootElement.GetProperty("status").GetString().ShouldBe("Degraded");
        var entry = body.RootElement.GetProperty("entries").GetProperty("rabbitmq");
        entry.GetProperty("status").GetString().ShouldBe("Degraded");
        entry.GetProperty("description").GetString().ShouldBe(description);
    }

    [Fact]
    public async Task WriteAsync_EntryWithException_ShouldOmitExceptionDetails()
    {
        // Arrange
        var exceptionMessage = _faker.Lorem.Sentence();
        var report = CreateReport(new HealthReportEntry(
            HealthStatus.Degraded,
            _faker.Lorem.Sentence(),
            TimeSpan.Zero,
            new InvalidOperationException(exceptionMessage),
            data: null));
        var context = CreateContext();

        // Act
        await HealthCheckResponseWriter.WriteAsync(context, report);

        // Assert
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.ShouldNotContain(exceptionMessage);
        body.ShouldNotContain(nameof(InvalidOperationException));
    }

    private static HealthReport CreateReport(HealthReportEntry entry) =>
        new(new Dictionary<string, HealthReportEntry> { ["rabbitmq"] = entry }, TimeSpan.Zero);

    private static DefaultHttpContext CreateContext() =>
        new() { Response = { Body = new MemoryStream() } };

    private static async Task<JsonDocument> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
