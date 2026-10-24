using Bogus;
using Ecommerce.Orders.Api.Authorization;
using Ecommerce.Orders.Api.Carts.GetMyCart;
using Ecommerce.Orders.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Orders.IntegrationTests.Carts;

public sealed class RemoveCartItemIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    private const string CartEndpoint = "/api/v1/carts/me";
    private const string ItemsEndpoint = "/api/v1/carts/me/items";
    private readonly Faker _faker = new();

    [Fact]
    public async Task Delete_WhenLineIsUnavailable_ShouldReturn204AndRemoveOnlyThatLine()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var deactivated = await SeedProductAsync();
        var kept = await SeedProductAsync();
        await SeedCartAsync(userId, (deactivated, 2), (kept, 1));
        await DeactivateProductAsync(deactivated);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.DeleteAsync($"{ItemsEndpoint}/{deactivated}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();

        var cart = (await GetCartsAsync()).ShouldHaveSingleItem();
        cart.Items.ShouldHaveSingleItem().ProductId.ShouldBe(kept);
    }

    [Fact]
    public async Task Delete_WhenProductIsNotInCart_ShouldReturn404WithProblemDetailsAndChangeNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var inCart = await SeedProductAsync();
        var notInCart = await SeedProductAsync();
        await SeedCartAsync(userId, (inCart, 2));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.DeleteAsync($"{ItemsEndpoint}/{notInCart}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe(404);

        var item = (await GetCartsAsync()).ShouldHaveSingleItem().Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(inCart);
        item.Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task Delete_WhenLastLineIsRemoved_ShouldReturnAnEmptyCartOnTheNextRead()
    {
        await ResetDatabaseAsync();

        // Arrange — the spec scenario is the sequence: remove the last line, then read the cart.
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync();
        await SeedCartAsync(userId, (productId, 3));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);
        var removal = await client.DeleteAsync($"{ItemsEndpoint}/{productId}");
        removal.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Act
        var response = await client.GetAsync(CartEndpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetMyCartResponse>();
        body.ShouldNotBeNull();
        body.Items.ShouldBeEmpty();
        body.TotalQuantity.ShouldBe(0);
        body.Total.ShouldBe(0m);
    }
}
