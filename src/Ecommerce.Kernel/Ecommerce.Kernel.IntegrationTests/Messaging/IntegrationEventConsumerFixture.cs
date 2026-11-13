using Ecommerce.Kernel.Infrastructure.Messaging;
using Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Kernel.IntegrationTests.Messaging;

public sealed class IntegrationEventConsumerFixture : MessagingFixture
{
    public const string AlphaRecordedQueue = "alpha.recorded-event";
    public const string BetaRecordedQueue = "beta.recorded-event";
    public const string WritesThenFailsQueue = "alpha.writes-then-fails-event";
    public const string AlwaysFailsQueue = "alpha.always-fails-event";
    public const string BusinessRuleQueue = "alpha.business-rule-event";
    public const string GatedQueue = "alpha.gated-event";

    protected override string[] ConsumerQueues =>
    [
        AlphaRecordedQueue, BetaRecordedQueue, WritesThenFailsQueue, AlwaysFailsQueue, BusinessRuleQueue, GatedQueue,
    ];

    protected override void ConfigureTestServices(IServiceCollection services) =>
        services
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
            .AddIntegrationEventHandler<GatedEvent, GatedHandler, MessagingTestDbContext>(GatedHandler.Module);
}
