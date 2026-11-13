namespace Ecommerce.Kernel.Infrastructure.Messaging.Outbox;

internal static class RelayBackoff
{
    public static TimeSpan Next(TimeSpan current, bool succeeded, OutboxOptions options)
    {
        if (succeeded)
        {
            return options.PollingInterval;
        }

        var doubled = TimeSpan.FromTicks(current.Ticks * 2);
        return doubled < options.MaxBackoff ? doubled : options.MaxBackoff;
    }
}
