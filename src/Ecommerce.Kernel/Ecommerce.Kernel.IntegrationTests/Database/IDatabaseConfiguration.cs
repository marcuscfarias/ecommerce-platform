namespace Ecommerce.Kernel.IntegrationTests.Database;

public interface IDatabaseConfiguration
{
    IReadOnlyDictionary<string, string?> GetConfigurationEntries();
}
