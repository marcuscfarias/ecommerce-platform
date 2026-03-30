using Ecommerce.Shared.IntegrationTests.Database;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogDatabaseConfiguration(string connectionString) : IDatabaseConfiguration
{
    public IReadOnlyDictionary<string, string?> GetConfigurationEntries() =>
        new Dictionary<string, string?> { ["ConnectionStrings:CatalogDb"] = connectionString };
}
