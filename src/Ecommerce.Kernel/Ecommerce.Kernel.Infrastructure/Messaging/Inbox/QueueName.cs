using System.Text.Json;

namespace Ecommerce.Kernel.Infrastructure.Messaging.Inbox;

internal static class QueueName
{
    private const string DeadLetterSuffix = ".dlq";

    public static string For(string module, string eventName) =>
        $"{module}.{JsonNamingPolicy.KebabCaseLower.ConvertName(eventName)}";

    public static string DeadLetterFor(string queue) => queue + DeadLetterSuffix;
}
