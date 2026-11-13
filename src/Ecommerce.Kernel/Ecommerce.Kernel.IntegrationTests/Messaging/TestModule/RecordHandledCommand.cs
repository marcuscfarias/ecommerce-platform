using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed record RecordHandledCommand(Guid EntityId, string Handler) : ICommand;
