using Ecommerce.Auth.Application.Auth.Security;
using Microsoft.Extensions.Options;

namespace Ecommerce.Auth.Infrastructure.Security;

internal sealed class EmailConfirmationTokenFactory(IOptions<EmailConfirmationSettings> settings)
    : IEmailConfirmationTokenFactory
{
    public TimeSpan Lifetime { get; } = TimeSpan.FromHours(settings.Value.TokenLifetimeHours);

    public EmailConfirmationTokenPair Create()
    {
        var plain = OpaqueToken.Generate();
        return new EmailConfirmationTokenPair(plain, Hash(plain));
    }

    public string Hash(string plain) => OpaqueToken.Hash(plain);
}
