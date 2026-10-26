using Bogus;
using Ecommerce.Orders.Api.Authorization;
using Ecommerce.Orders.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Orders.IntegrationTests.Carts;

public sealed class AddCartItemIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/carts/me/items";
    private readonly Faker _faker = new();

    [Fact]
    public async Task Post_WhenCustomerHasNoCart_ShouldReturn204AndCreateCartWithTheLine()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 2 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();

        var cart = (await GetCartsAsync()).ShouldHaveSingleItem();
        cart.UserId.ShouldBe(userId);
        var item = cart.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(productId);
        item.Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task Post_WhenProductIsAddedTwice_ShouldKeepOneLineWithTheSummedQuantity()
    {
        await ResetDatabaseAsync();

        // Arrange — the spec scenario is the sequence: add 1, then add 2 of the same product.
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);
        await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 1 });

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 2 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var cart = (await GetCartsAsync()).ShouldHaveSingleItem();
        var item = cart.Items.ShouldHaveSingleItem();
        item.ProductId.ShouldBe(productId);
        item.Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task Post_WhenProductPriceIsZero_ShouldReturn204AndAddTheLine()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(price: 0m, stockQuantity: 5);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 1 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await GetCartsAsync()).ShouldHaveSingleItem().Items.ShouldHaveSingleItem().ProductId.ShouldBe(productId);
    }

    [Fact]
    public async Task Post_WhenQuantityIsAboveTen_ShouldReturn400WithValidationProblemDetailsAndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 20);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 11 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Quantity");

        (await GetCartsAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Post_WhenProductDoesNotExist_ShouldReturn404WithProblemDetailsAndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId = 999999, quantity = 1 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe(404);

        (await GetCartsAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Post_WhenProductIsInactive_ShouldReturn404WithProblemDetailsAndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 5);
        await DeactivateProductAsync(productId);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 1 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe(404);

        (await GetCartsAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Post_WhenResultingQuantityExceedsTenAndStock_ShouldReturn409WithMaximumDetailAndKeepTheLine()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 2);
        await SeedCartAsync(userId, (productId, 8));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 3 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Maximum of 10 units per product.");

        (await GetCartsAsync()).ShouldHaveSingleItem().Items.ShouldHaveSingleItem().Quantity.ShouldBe(8);
    }

    [Fact]
    public async Task Post_WhenProductIsOutOfStock_ShouldReturn409WithOutOfStockDetailAndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 0);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 1 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Product is out of stock.");

        (await GetCartsAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Post_WhenQuantityExceedsStock_ShouldReturn409WithAvailableQuantityDetailAndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(stockQuantity: 2);
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, new { productId, quantity = 5 });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Detail.ShouldBe("Only 2 available.");

        (await GetCartsAsync()).ShouldBeEmpty();
    }
}
