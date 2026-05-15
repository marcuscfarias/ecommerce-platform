using System.Globalization;

namespace Ecommerce.Kernel.IntegrationTests;

// AppHost validates Auth:Jwt:* on startup, so every bounded-context's test host
// needs these values present to boot — not just Auth's own integration tests.
public static class TestJwtDefaults
{
    public const string SecretKey = "integration-test-secret-key-with-at-least-32-bytes-for-hs256!!";
    public const string Issuer = "ecommerce-platform";
    public const string Audience = "ecommerce-platform";
    public const int AccessTokenMinutes = 15;

    public static IReadOnlyDictionary<string, string?> AsConfiguration() => new Dictionary<string, string?>
    {
        ["Auth:Jwt:SecretKey"] = SecretKey,
        ["Auth:Jwt:Issuer"] = Issuer,
        ["Auth:Jwt:Audience"] = Audience,
        ["Auth:Jwt:AccessTokenMinutes"] = AccessTokenMinutes.ToString(CultureInfo.InvariantCulture),
    };
}
