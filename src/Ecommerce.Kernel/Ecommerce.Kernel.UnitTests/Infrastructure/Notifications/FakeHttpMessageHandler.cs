namespace Ecommerce.Kernel.UnitTests.Infrastructure.Notifications;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage? _response;
    private readonly Exception? _exception;

    public FakeHttpMessageHandler(HttpResponseMessage response) => _response = response;

    public FakeHttpMessageHandler(Exception exception) => _exception = exception;

    public HttpRequestMessage? Request { get; private set; }

    public string? RequestBody { get; private set; }

    public int RequestCount { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Request = request;
        RequestCount++;

        if (request.Content is not null)
        {
            RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        if (_exception is not null)
        {
            throw _exception;
        }

        return _response!;
    }
}
