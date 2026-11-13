using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed record GatedCommand(Guid EntityId, bool Held, string Handler) : ICommand;
