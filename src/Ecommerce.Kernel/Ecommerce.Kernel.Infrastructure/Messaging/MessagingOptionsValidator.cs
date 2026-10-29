using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal sealed class MessagingOptionsValidator(IHostEnvironment environment) : IValidateOptions<MessagingOptions>
{
    private const string SecureScheme = "amqps";

    public ValidateOptionsResult Validate(string? name, MessagingOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.RabbitMqConnectionString))
        {
            return ValidateOptionsResult.Fail("ConnectionStrings:RabbitMq is required.");
        }

        if (environment.IsProduction() && !UsesSecureScheme(options.RabbitMqConnectionString))
        {
            return ValidateOptionsResult.Fail("ConnectionStrings:RabbitMq must use the amqps scheme in Production.");
        }

        return ValidateOptionsResult.Success;
    }

    private static bool UsesSecureScheme(string connectionString) =>
        Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
        && uri.Scheme.Equals(SecureScheme, StringComparison.OrdinalIgnoreCase);
}
