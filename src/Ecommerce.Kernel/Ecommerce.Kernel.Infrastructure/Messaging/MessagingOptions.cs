namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal sealed class MessagingOptions
{
    public const string SectionName = "Messaging";
    public const string ConnectionStringName = "RabbitMq";

    public bool Enabled { get; init; }
    public string ConnectionString { get; set; } = string.Empty;
    public OutboxOptions Outbox { get; init; } = new();
    public ConsumerOptions Consumer { get; init; } = new();
}
