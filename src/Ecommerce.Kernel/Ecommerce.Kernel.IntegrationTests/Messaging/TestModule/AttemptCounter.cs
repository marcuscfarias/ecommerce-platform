using System.Collections.Concurrent;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class AttemptCounter
{
    private readonly ConcurrentDictionary<Guid, int> _attempts = new();

    public int Increment(Guid entityId) => _attempts.AddOrUpdate(entityId, 1, (_, attempts) => attempts + 1);

    public int Get(Guid entityId) => _attempts.GetValueOrDefault(entityId);
}
