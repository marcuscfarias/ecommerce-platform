namespace Ecommerce.Kernel.Infrastructure.Messaging;

internal sealed class MessagingOptions
{
    public const string SectionName = "Messaging";
    public const string ConnectionStringName = "RabbitMq";

    public static readonly IReadOnlyList<TimeSpan> DefaultRetryCooldowns =
    [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(12),
        TimeSpan.FromSeconds(30),
    ];

    public IReadOnlyList<TimeSpan> RetryCooldowns { get; set; } = [];
    public TimeSpan DuplicateDetectionWindow { get; set; } = TimeSpan.FromHours(24);
    public string RabbitMqConnectionString { get; set; } = string.Empty;
}
