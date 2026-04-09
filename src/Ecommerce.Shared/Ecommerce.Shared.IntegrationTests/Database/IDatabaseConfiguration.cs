namespace Ecommerce.Shared.IntegrationTests.Database;

public interface IDatabaseConfiguration
{
    IReadOnlyDictionary<string, string?> GetConfigurationEntries();
}
