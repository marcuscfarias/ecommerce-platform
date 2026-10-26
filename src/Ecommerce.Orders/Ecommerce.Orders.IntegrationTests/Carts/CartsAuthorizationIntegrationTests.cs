using Bogus;
using Ecommerce.Orders.Api.Authorization;
using Ecommerce.Orders.Api.Carts.GetMyCart;
using Ecommerce.Orders.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Orders.IntegrationTests.Carts;

public sealed class CartsAuthorizationIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    private const string CartEndpoint = "/api/v1/carts/me";
    private const string ItemsEndpoint = "/api/v1/carts/me/items";
    private const string OperatorPermission = "catalog:manage";
    private readonly Faker _faker = new();

    [Theory]
    [InlineData("GET", CartEndpoint)]            // read the cart
    [InlineData("POST", ItemsEndpoint)]          // add an item
    [InlineData("PUT", ItemsEndpoint + "/1")]    // change a quantity
    [InlineData("DELETE", ItemsEndpoint + "/1")] // remove an item
    public async Task Request_WhenNoToken_ShouldReturn401AndPersistNoCart(string method, string path)
    {
        await ResetDatabaseAsync();

        // Arrange
        using var request = CreateRequest(method, path);

        // Act
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await GetCartsAsync()).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("GET", CartEndpoint)]            // read the cart
    [InlineData("POST", ItemsEndpoint)]          // add an item
    [InlineData("PUT", ItemsEndpoint + "/1")]    // change a quantity
    [InlineData("DELETE", ItemsEndpoint + "/1")] // remove an item
    public async Task Request_WhenTokenLacksCartPermission_ShouldReturn403AndPersistNoCart(string method, string path)
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        using var client = CreateCustomerClient(userId, OperatorPermission);
        using var request = CreateRequest(method, path);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await GetCartsAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Get_WhenAnotherCustomerHasACart_ShouldReturnOnlyTheCallersLines()
    {
        await ResetDatabaseAsync();

        // Arrange
        var callerId = await SeedAuthUserAsync(_faker.Name.FullName());
        var otherId = await SeedAuthUserAsync(_faker.Name.FullName());
        var callersProduct = await SeedProductAsync();
        var othersProduct = await SeedProductAsync();
        await SeedCartAsync(callerId, (callersProduct, 1));
        await SeedCartAsync(otherId, (othersProduct, 2));
        using var client = CreateCustomerClient(callerId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.GetAsync(CartEndpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetMyCartResponse>();
        body.ShouldNotBeNull();
        var item = body.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(callersProduct);
        item.Quantity.ShouldBe(1);
    }

    [Fact]
    public async Task Delete_WhenProductIsOnlyInAnotherCustomersCart_ShouldReturn404AndKeepTheirLine()
    {
        await ResetDatabaseAsync();

        // Arrange
        var callerId = await SeedAuthUserAsync(_faker.Name.FullName());
        var otherId = await SeedAuthUserAsync(_faker.Name.FullName());
        var othersProduct = await SeedProductAsync();
        await SeedCartAsync(callerId);
        await SeedCartAsync(otherId, (othersProduct, 2));
        using var client = CreateCustomerClient(callerId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.DeleteAsync($"{ItemsEndpoint}/{othersProduct}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe(404);

        var othersCart = (await GetCartsAsync()).Single(c => c.UserId == otherId);
        var item = othersCart.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(othersProduct);
        item.Quantity.ShouldBe(2);
    }

    private static HttpRequestMessage CreateRequest(string method, string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { productId = 1, quantity = 1 });

        return request;
    }
}
