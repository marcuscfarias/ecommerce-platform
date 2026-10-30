using Ecommerce.Kernel.IntegrationTests.Database;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class MessagingDatabaseConfiguration(string connectionString) : IDatabaseConfiguration
{
    public IReadOnlyDictionary<string, string?> GetConfigurationEntries() =>
        new Dictionary<string, string?> { ["ConnectionStrings:EcommerceDb"] = connectionString };
}
