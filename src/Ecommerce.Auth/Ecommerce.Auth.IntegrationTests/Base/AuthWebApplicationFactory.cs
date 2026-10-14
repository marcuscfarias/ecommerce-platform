using Ecommerce.Auth.IntegrationTests.Base.Database;
using Ecommerce.Kernel.Application.Notifications;
using Ecommerce.Kernel.IntegrationTests;
using Ecommerce.Kernel.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ecommerce.Auth.IntegrationTests.Base;

public sealed class AuthWebApplicationFactory(DatabaseContainerFixture containerFixture)
    : EcommerceWebApplicationFactory(new AuthDatabaseConfiguration(containerFixture.ConnectionString))
{
    public CapturingEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }
}
