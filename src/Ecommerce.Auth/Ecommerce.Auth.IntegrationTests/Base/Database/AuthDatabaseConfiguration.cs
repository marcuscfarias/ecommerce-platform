using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Auth.IntegrationTests.Base.Database;

public sealed class AuthDatabaseConfiguration(string connectionString) : IDatabaseConfiguration
{
    public IReadOnlyDictionary<string, string?> GetConfigurationEntries() =>
        new Dictionary<string, string?> { ["ConnectionStrings:EcommerceDb"] = connectionString };
}
