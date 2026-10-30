using Microsoft.AspNetCore.Hosting;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class MessagingWebApplicationFactory(string databaseConnectionString, string rabbitMqConnectionString)
    : EcommerceWebApplicationFactory(new MessagingDatabaseConfiguration(databaseConnectionString))
{
    protected override void ConfigureMessaging(IWebHostBuilder builder)
    {
        // Real broker from the RabbitMQ container
        builder.UseSetting("ConnectionStrings:RabbitMq", rabbitMqConnectionString);
    }
}
