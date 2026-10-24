using System.Text.Json;
using Bogus;
using Ecommerce.Orders.Api.Authorization;
using Ecommerce.Orders.Api.Carts.GetMyCart;
using Ecommerce.Orders.IntegrationTests.Base;

namespace Ecommerce.Orders.IntegrationTests.Carts;

public sealed class GetMyCartIntegrationTests(OrdersIntegrationFixture fixture)
    : BaseOrdersIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/carts/me";
    private readonly Faker _faker = new();

    [Fact]
    public async Task Get_WhenCustomerHasNoCart_ShouldReturn200WithEmptyCartAndPersistNoCart()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        payload.RootElement.EnumerateObject().Select(p => p.Name)
            .ShouldBe(["items", "totalQuantity", "total"], ignoreOrder: true);

        var body = await response.Content.ReadFromJsonAsync<GetMyCartResponse>();
        body.ShouldNotBeNull();
        body.Items.ShouldBeEmpty();
        body.TotalQuantity.ShouldBe(0);
        body.Total.ShouldBe(0m);

        (await GetCartsAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Get_WhenCatalogChangedAfterAdding_ShouldReturnEachStatusTheNewPriceAndTotals()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var repriced = await SeedProductAsync(price: 10.00m, stockQuantity: 10);
        var soldOut = await SeedProductAsync(price: 20.00m, stockQuantity: 10);
        var lowStock = await SeedProductAsync(price: 30.00m, stockQuantity: 10);
        var deactivated = await SeedProductAsync(price: 40.00m, stockQuantity: 10);
        await SeedCartAsync(userId, (repriced, 2), (soldOut, 1), (lowStock, 3), (deactivated, 1));

        await UpdateProductAsync(repriced, price: 15.00m, stockQuantity: 10);
        await UpdateProductAsync(soldOut, price: 20.00m, stockQuantity: 0);
        await UpdateProductAsync(lowStock, price: 30.00m, stockQuantity: 1);
        await DeactivateProductAsync(deactivated);

        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetMyCartResponse>();
        body.ShouldNotBeNull();
        body.Items.Select(i => (i.ProductId, i.Status, i.UnitPrice, i.Quantity, i.Subtotal, i.AvailableQuantity)).ShouldBe([
            (repriced, "available", 15.00m, 2, 30.00m, (int?)null),
            (soldOut, "outOfStock", 20.00m, 1, 20.00m, null),
            (lowStock, "insufficientStock", 30.00m, 3, 90.00m, 1),
            (deactivated, "unavailable", 40.00m, 1, 40.00m, null),
        ]);
        body.TotalQuantity.ShouldBe(7);
        body.Total.ShouldBe(30.00m);
    }

    [Fact]
    public async Task Get_WhenCartHasLine_ShouldReturnTheExactItemFieldSet()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync();
        await SeedCartAsync(userId, (productId, 1));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = payload.RootElement.GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        item.EnumerateObject().Select(p => p.Name).ShouldBe(
            ["productId", "name", "unitPrice", "quantity", "subtotal", "hasImage", "status", "availableQuantity"],
            ignoreOrder: true);
        item.GetProperty("availableQuantity").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Get_WhenProductPriceIsZero_ShouldReturnZeroSubtotal()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var productId = await SeedProductAsync(price: 0m, stockQuantity: 5);
        await SeedCartAsync(userId, (productId, 2));
        using var client = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetMyCartResponse>();
        body.ShouldNotBeNull();
        var item = body.Items.ShouldHaveSingleItem();
        item.Status.ShouldBe("available");
        item.Subtotal.ShouldBe(0m);
        body.Total.ShouldBe(0m);
    }

    [Fact]
    public async Task Get_WhenCustomerSignsInAgain_ShouldReturnTheSameLinesAndQuantities()
    {
        await ResetDatabaseAsync();

        // Arrange — the spec scenario is the sequence: read, sign in again with a new token, read.
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var first = await SeedProductAsync();
        var second = await SeedProductAsync();
        await SeedCartAsync(userId, (first, 2), (second, 1));

        GetMyCartResponse? beforeSignOut;
        using (var firstSession = CreateCustomerClient(userId, OrdersPermissions.ManageCart))
            beforeSignOut = await firstSession.GetFromJsonAsync<GetMyCartResponse>(Endpoint);

        using var secondSession = CreateCustomerClient(userId, OrdersPermissions.ManageCart);

        // Act
        var response = await secondSession.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var afterSignIn = await response.Content.ReadFromJsonAsync<GetMyCartResponse>();
        afterSignIn.ShouldNotBeNull();
        afterSignIn.Items.Select(i => (i.ProductId, i.Quantity)).ShouldBe([(first, 2), (second, 1)]);
        beforeSignOut.ShouldNotBeNull().Items.Select(i => (i.ProductId, i.Quantity)).ShouldBe([(first, 2), (second, 1)]);
    }
}
