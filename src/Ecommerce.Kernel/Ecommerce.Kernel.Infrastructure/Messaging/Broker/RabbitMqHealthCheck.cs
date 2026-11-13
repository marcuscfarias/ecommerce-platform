using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Broker;

internal sealed class RabbitMqHealthCheck(
    RabbitMqConnection connection,
    IOptions<MessagingOptions> options) : IHealthCheck
{
    public const string Name = "rabbitmq";

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!options.Value.Enabled)
        {
            return Task.FromResult(HealthCheckResult.Healthy("messaging disabled"));
        }

        return Task.FromResult(connection.IsOpen
            ? HealthCheckResult.Healthy("broker connected")
            : new HealthCheckResult(context.Registration.FailureStatus, "broker unreachable"));
    }
}
