namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal static class IntegrationEventName
{
    public static string Of<TEvent>() => typeof(TEvent).Name;
}
