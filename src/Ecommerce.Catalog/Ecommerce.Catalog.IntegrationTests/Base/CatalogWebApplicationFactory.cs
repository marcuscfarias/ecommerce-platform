using Ecommerce.Catalog.IntegrationTests.Base.Database;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Catalog.IntegrationTests.Base;

public sealed class CatalogWebApplicationFactory(DatabaseContainerFixture containerFixture, string blobConnectionString)
    : EcommerceWebApplicationFactory(new CatalogDatabaseConfiguration(containerFixture.ConnectionString))
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Point the product image storage at the Azurite container. ContainerName falls back
        // to the options default ("product-images").
        builder.ConfigureAppConfiguration(cfg =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProductImageStorage:ConnectionString"] = blobConnectionString,
            }));
    }
}
