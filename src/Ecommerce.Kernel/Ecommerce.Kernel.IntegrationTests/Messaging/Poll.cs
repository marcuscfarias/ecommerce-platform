namespace Ecommerce.Kernel.IntegrationTests.Messaging;

internal static class Poll
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(250);

    public static async Task<T> UntilAsync<T>(Func<Task<T>> probe, Func<T, bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        var value = await probe();

        while (!condition(value) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(Interval);
            value = await probe();
        }

        return value;
    }
}
