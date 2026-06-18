using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests.Base.Database;

public sealed class CatalogDatabaseConfiguration(string connectionString) : IDatabaseConfiguration
{
    public IReadOnlyDictionary<string, string?> GetConfigurationEntries() =>
        new Dictionary<string, string?> { ["ConnectionStrings:EcommerceDb"] = connectionString };
}
