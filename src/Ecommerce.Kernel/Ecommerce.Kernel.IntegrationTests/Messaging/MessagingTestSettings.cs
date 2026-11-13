namespace Ecommerce.Kernel.IntegrationTests.Messaging;

internal static class MessagingTestSettings
{
    public static Dictionary<string, string?> Enabled(string connectionString) => new()
    {
        ["Messaging:Enabled"] = "true",
        ["ConnectionStrings:RabbitMq"] = connectionString,
        ["Messaging:Outbox:PollingInterval"] = "00:00:00.100",
        ["Messaging:Outbox:MaxBackoff"] = "00:00:01",
        ["Messaging:Outbox:PublishTimeout"] = "00:00:02",
        ["Messaging:Consumer:RetryBaseDelay"] = "00:00:00.010",
        ["Messaging:Consumer:RetryMaxDelay"] = "00:00:00.050",
    };
}
