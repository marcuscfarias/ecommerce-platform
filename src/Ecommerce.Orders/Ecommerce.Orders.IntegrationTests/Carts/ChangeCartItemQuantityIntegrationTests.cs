using Bogus;
using Ecommerce.Orders.Api.Authorization;
using Ecommerce.Orders.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Orders.IntegrationTests.Carts;

public sealed class ChangeCartItemQuantityIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/carts/me/items";
    private readonly Faker _faker = new();

    [Fact]
    public async Task Put_WhenStockCoversQuantity_ShouldReturn204AndReplaceTheQuantity()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        await SeedCartAsync(userId, (productId, 1));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/{productId}", new { quantity = 4 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();
        (await GetLineQuantityAsync()).ShouldBe(4);
    }

    [Fact]
    public async Task Put_WhenQuantityIsZero_ShouldReturn400WithValidationProblemDetailsAndKeepTheQuantity()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        await SeedCartAsync(userId, (productId, 2));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/{productId}", new { quantity = 0 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Quantity");

        (await GetLineQuantityAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Put_WhenProductIsNotInCart_ShouldReturn404WithProblemDetailsAndChangeNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var inCart = await SeedProductAsync(stockQuantity: 5);
        var notInCart = await SeedProductAsync(stockQuantity: 5);
        await SeedCartAsync(userId, (inCart, 2));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/{notInCart}", new { quantity = 3 });

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
    public async Task Put_WhenProductIsInactive_ShouldReturn409WithNoLongerAvailableDetailAndKeepTheQuantity()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        await SeedCartAsync(userId, (productId, 2));
        await DeactivateProductAsync(productId);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/{productId}", new { quantity = 1 });

        // Assert
        await AssertConflictAsync(response, "Product is no longer available.");
        (await GetLineQuantityAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Put_WhenProductIsOutOfStock_ShouldReturn409WithOutOfStockDetailAndKeepTheQuantity()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        await SeedCartAsync(userId, (productId, 2));
        await UpdateProductAsync(productId, price: 10.00m, stockQuantity: 0);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/{productId}", new { quantity = 1 });

        // Assert
        await AssertConflictAsync(response, "Product is out of stock.");
        (await GetLineQuantityAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Put_WhenQuantityExceedsStock_ShouldReturn409WithAvailableQuantityDetailAndKeepTheQuantity()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        await SeedCartAsync(userId, (productId, 4));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/{productId}", new { quantity = 6 });

        // Assert
        await AssertConflictAsync(response, "Only 5 available.");
        (await GetLineQuantityAsync()).ShouldBe(4);
    }

    [Fact]
    public async Task Put_WhenDecreaseStillExceedsStock_ShouldReturn409WithAvailableQuantityDetailAndKeepTheQuantity()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 10);
        await SeedCartAsync(userId, (productId, 5));
        await UpdateProductAsync(productId, price: 10.00m, stockQuantity: 3);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/{productId}", new { quantity = 4 });

        // Assert
        await AssertConflictAsync(response, "Only 3 available.");
        (await GetLineQuantityAsync()).ShouldBe(5);
    }

    private static async Task AssertConflictAsync(HttpResponseMessage response, string detail)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe(detail);
    }

    private async Task<int> GetLineQuantityAsync() =>
        (await GetCartsAsync()).ShouldHaveSingleItem().Items.ShouldHaveSingleItem().Quantity;
}
