namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class HandledMessage
{
    public Guid Id { get; init; }
    public Guid EntityId { get; init; }
    public string Handler { get; init; } = string.Empty;
}
