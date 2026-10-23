using System.Text.Json;
using Bogus;
using Ecommerce.Customers.Api.Authorization;
using Ecommerce.Customers.Api.Profiles.GetMyProfile;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Infrastructure.Persistence;
using Ecommerce.Customers.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customers.IntegrationTests.Profiles;

public sealed class GetMyProfileIntegrationTests(CustomersIntegrationFixture fixture)
    : BaseCustomersIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/customers/me";
    private readonly Faker _faker = new();

    [Fact]
    public async Task Get_WhenCustomerHasNoProfile_ShouldReturn200WithAuthNameAndPersistOneProfile()
    {
        await ResetDatabaseAsync();

        // Arrange
        var name = _faker.Name.FullName();
        var userId = await SeedAuthUserAsync(name);
        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.EnumerateObject().Select(p => p.Name).ShouldBe(["displayName", "phone"], ignoreOrder: true);
        body.GetProperty("displayName").GetString().ShouldBe(name);
        body.GetProperty("phone").ValueKind.ShouldBe(JsonValueKind.Null);

        var customers = await GetCustomersByUserIdAsync(userId);
        var customer = customers.ShouldHaveSingleItem();
        customer.DisplayName.ShouldBe(name);
        customer.Phone.ShouldBeNull();
    }

    [Fact]
    public async Task Get_WhenCustomerAlreadyHasProfile_ShouldReturn200WithStoredValuesAndKeepOneProfile()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var storedName = _faker.Name.FullName();
        var storedPhone = _faker.Phone.PhoneNumber("+55###########");
        var seeded = new Customer(userId, storedName);
        seeded.UpdateProfile(storedName, storedPhone);
        await SeedAsync(db =>
        {
            db.Customers.Add(seeded);
            return Task.CompletedTask;
        });

        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<GetMyProfileResponse>();
        body.ShouldNotBeNull();
        body.DisplayName.ShouldBe(storedName);
        body.Phone.ShouldBe(storedPhone);

        var customers = await GetCustomersByUserIdAsync(userId);
        customers.ShouldHaveSingleItem().Id.ShouldBe(seeded.Id);
    }

    [Fact]
    public async Task Get_WhenAuthUserDoesNotExist_ShouldReturn404WithProblemDetailsAndPersistNothing()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = _faker.Random.Int(1_000_000, int.MaxValue);
        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe(404);

        var customers = await GetCustomersByUserIdAsync(userId);
        customers.ShouldBeEmpty();
    }

    private async Task<List<Customer>> GetCustomersByUserIdAsync(int userId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        return await db.Customers.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
    }
}
