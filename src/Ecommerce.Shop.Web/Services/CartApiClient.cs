using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.Shop.Web.Services;

public sealed class CartApiClient(HttpClient httpClient)
{
    private const string CartPath = "api/v1/carts/me";
    private const string ItemsPath = "api/v1/carts/me/items";
    private const string FallbackError = "Something went wrong. Please try again.";
    private const string NetworkError = "We couldn't reach the server. Check your connection and try again.";

    public async Task<Cart?> GetCartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(CartPath, cancellationToken);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<Cart>(cancellationToken)
                : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
        {
            return null;
        }
    }

    public Task<CartOutcome> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(() => httpClient.PostAsJsonAsync(ItemsPath, request, cancellationToken), cancellationToken);

    public Task<CartOutcome> ChangeItemQuantityAsync(
        int productId,
        ChangeCartItemQuantityRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync(() => httpClient.PutAsJsonAsync(ItemPath(productId), request, cancellationToken), cancellationToken);

    public Task<CartOutcome> RemoveItemAsync(int productId, CancellationToken cancellationToken = default) =>
        SendAsync(() => httpClient.DeleteAsync(ItemPath(productId), cancellationToken), cancellationToken);

    private static string ItemPath(int productId) =>
        $"{ItemsPath}/{productId.ToString(CultureInfo.InvariantCulture)}";

    private static async Task<CartOutcome> SendAsync(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await send();
        }
        catch (HttpRequestException)
        {
            return CartOutcome.Failure(NetworkError);
        }

        return response.IsSuccessStatusCode
            ? CartOutcome.Success()
            : CartOutcome.Failure(await ReadErrorAsync(response, cancellationToken));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken);

            if (problem?.Errors is { Count: > 0 } errors)
            {
                return string.Join(" ", errors.Values.SelectMany(messages => messages));
            }

            return string.IsNullOrWhiteSpace(problem?.Detail) ? FallbackError : problem.Detail;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return FallbackError;
        }
    }

    private sealed record ProblemResponse(string? Detail, IReadOnlyDictionary<string, string[]>? Errors);
}
