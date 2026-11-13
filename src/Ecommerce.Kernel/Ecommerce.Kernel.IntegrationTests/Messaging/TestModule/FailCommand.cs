using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed record FailCommand(Guid EntityId, bool BreaksBusinessRule) : ICommand;
