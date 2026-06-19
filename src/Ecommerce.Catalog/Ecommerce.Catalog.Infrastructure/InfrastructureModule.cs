using Azure.Identity;
using Azure.Storage.Blobs;
using Ecommerce.Catalog.Application;
using Ecommerce.Catalog.Domain.Repositories;
using Ecommerce.Catalog.Domain.Storage;
using Ecommerce.Catalog.Infrastructure.Mediation;
using Ecommerce.Catalog.Infrastructure.Persistence;
using Ecommerce.Catalog.Infrastructure.Persistence.Repositories;
using Ecommerce.Catalog.Infrastructure.Persistence.Storage;
using Ecommerce.Kernel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Catalog.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<CatalogDbContext>(CatalogDbContext.Schema);

        services.AddMediationModule();

        services.AddScoped<ICatalogModule, CatalogModule>();
        services.AddScoped<ICatalogRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.Configure<ProductImageStorageOptions>(configuration.GetSection(ProductImageStorageOptions.SectionName));
        services.AddSingleton(CreateBlobServiceClient);
        services.AddScoped<IProductImageStorage, AzureBlobProductImageStorage>();

        return services;
    }

    private static BlobServiceClient CreateBlobServiceClient(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<ProductImageStorageOptions>>().Value;

        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            return new BlobServiceClient(options.ConnectionString);

        if (string.IsNullOrWhiteSpace(options.ServiceUri))
            throw new InvalidOperationException(
                $"{ProductImageStorageOptions.SectionName}: either 'ConnectionString' (local/Azurite) or " +
                "'ServiceUri' (managed identity) must be configured.");

        return new BlobServiceClient(new Uri(options.ServiceUri), new DefaultAzureCredential());
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, bool applyMigrations = false)
    {
        if (!applyMigrations)
            return app;

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        context.Database.Migrate();
        return app;
    }
}
