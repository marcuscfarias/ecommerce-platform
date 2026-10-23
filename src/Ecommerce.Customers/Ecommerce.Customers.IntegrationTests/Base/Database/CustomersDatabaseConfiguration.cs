using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Customers.IntegrationTests.Base.Database;

public sealed class CustomersDatabaseConfiguration(string connectionString) : IDatabaseConfiguration
{
    public IReadOnlyDictionary<string, string?> GetConfigurationEntries() =>
        new Dictionary<string, string?> { ["ConnectionStrings:EcommerceDb"] = connectionString };
}
