using Ecommerce.Kernel.Infrastructure.Messaging;
using Ecommerce.Kernel.Infrastructure.Messaging.Inbox;
using Ecommerce.Kernel.IntegrationTests.Database;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class IntegrationEventConsumerFixture : BaseIntegrationFixture<MessagingWebApplicationFactory>
{
    public const string AlphaRecordedQueue = "alpha.recorded-event";
    public const string BetaRecordedQueue = "beta.recorded-event";
    public const string WritesThenFailsQueue = "alpha.writes-then-fails-event";
    public const string AlwaysFailsQueue = "alpha.always-fails-event";
    public const string BusinessRuleQueue = "alpha.business-rule-event";
    public const string GatedQueue = "alpha.gated-event";

    private static readonly string[] Queues =
    [
        AlphaRecordedQueue, BetaRecordedQueue, WritesThenFailsQueue, AlwaysFailsQueue, BusinessRuleQueue, GatedQueue,
    ];
    private static readonly TimeSpan TopologyDeadline = TimeSpan.FromSeconds(30);

    public RabbitMqContainerFixture Broker { get; } = new();

    public FakeLogCollector Logs => Factory.Services.GetFakeLogCollector();

    internal AttemptCounter Attempts => Factory.Services.GetRequiredService<AttemptCounter>();

    internal RetryGate Gate => Factory.Services.GetRequiredService<RetryGate>();

    protected override string[] Schemas => [MessagingTestDbContext.Schema];

    public override async Task InitializeAsync()
    {
        await Broker.InitializeAsync();
        await base.InitializeAsync();

        var declared = await Poll.UntilAsync(
            () => Broker.ListQueuesAsync("name"),
            output => Queues.All(queue => output.Contains(queue, StringComparison.Ordinal)),
            TopologyDeadline);
        Queues.ShouldAllBe(queue => declared.Contains(queue, StringComparison.Ordinal));
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await Broker.DisposeAsync();
    }

    internal async Task<List<HandledMessage>> HandledMessagesForAsync(Guid entityId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        return await db.HandledMessages.Where(message => message.EntityId == entityId).ToListAsync();
    }

    internal async Task<List<InboxMessage>> InboxMessagesForAsync(Guid messageId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MessagingTestDbContext>();
        return await db.Set<InboxMessage>().Where(message => message.MessageId == messageId).ToListAsync();
    }

    protected override MessagingWebApplicationFactory CreateFactory(DatabaseContainerFixture container) =>
        new(
            container,
            MessagingTestSettings.Enabled(Broker.ConnectionString),
            services => services
                .AddFakeLogging()
                .AddMessagingTestModule()
                .AddIntegrationEventHandler<RecordedEvent, AlphaRecordedHandler, MessagingTestDbContext>(
                    AlphaRecordedHandler.Module)
                .AddIntegrationEventHandler<RecordedEvent, BetaRecordedHandler, MessagingTestDbContext>(
                    BetaRecordedHandler.Module)
                .AddIntegrationEventHandler<WritesThenFailsEvent, WritesThenFailsHandler, MessagingTestDbContext>(
                    WritesThenFailsHandler.Module)
                .AddIntegrationEventHandler<AlwaysFailsEvent, AlwaysFailsHandler, MessagingTestDbContext>(
                    AlwaysFailsHandler.Module)
                .AddIntegrationEventHandler<BusinessRuleEvent, BusinessRuleHandler, MessagingTestDbContext>(
                    BusinessRuleHandler.Module)
                .AddIntegrationEventHandler<GatedEvent, GatedHandler, MessagingTestDbContext>(GatedHandler.Module));
}
