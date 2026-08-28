namespace Ecommerce.Shop.Web.Services;

// The API scales to zero when idle, so the first request after a quiet period hangs for
// about a minute. A request outliving the threshold is the only signal the SPA gets.
internal sealed class ColdStartHandler(ColdStartNotice notice) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var send = base.SendAsync(request, cancellationToken);
        var threshold = Task.Delay(ColdStartNotice.Threshold, cancellationToken);

        // A cancelled threshold means the caller walked away, not that the API is slow.
        if (await Task.WhenAny(send, threshold) == threshold && threshold.IsCompletedSuccessfully)
        {
            notice.Signal();
        }

        return await send;
    }
}
