using System.Globalization;

namespace Ecommerce.Kernel.IntegrationTests;

// The host validates Jwt:* on startup, so every bounded-context's test host needs these
// values present to boot. Injecting them also gives tests a known signing key, so
// TestTokenFactory can mint tokens this host will accept regardless of appsettings.
public static class TestJwtDefaults
{
    public const string SecretKey = "integration-test-secret-key-with-at-least-32-bytes-for-hs256!!";
    public const string Issuer = "ecommerce-platform";
    public const string Audience = "ecommerce-platform";
    public const int AccessTokenMinutes = 15;

    public static IReadOnlyDictionary<string, string?> AsConfiguration() => new Dictionary<string, string?>
    {
        ["Jwt:SecretKey"] = SecretKey,
        ["Jwt:Issuer"] = Issuer,
        ["Jwt:Audience"] = Audience,
        ["Jwt:AccessTokenMinutes"] = AccessTokenMinutes.ToString(CultureInfo.InvariantCulture),
    };
}
