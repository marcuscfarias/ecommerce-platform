using System.Collections.Concurrent;

namespace Ecommerce.Kernel.IntegrationTests.Messaging.TestModule;

internal sealed class RetryGate
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource> _gates = new();

    public Task WaitAsync(Guid entityId, CancellationToken cancellationToken) =>
        GateFor(entityId).Task.WaitAsync(cancellationToken);

    public void Release(Guid entityId) => GateFor(entityId).TrySetResult();

    private TaskCompletionSource GateFor(Guid entityId) =>
        _gates.GetOrAdd(entityId, _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
}
