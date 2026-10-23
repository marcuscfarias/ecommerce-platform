using Bogus;
using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Users.GetUserById;
using Ecommerce.Customers.Api.Authorization;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Infrastructure.Persistence;
using Ecommerce.Customers.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customers.IntegrationTests.Profiles;

public sealed class UpdateMyProfileIntegrationTests(CustomersIntegrationFixture fixture)
    : BaseCustomersIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/customers/me";
    private const string PhoneFormat = "+55###########";
    private readonly Faker _faker = new();

    [Fact]
    public async Task Put_WhenCustomerAlreadyHasProfile_ShouldReturn204AndReplaceStoredValues()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var seeded = await SeedCustomerAsync(userId);
        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);
        var displayName = _faker.Name.FullName();
        var phone = _faker.Phone.PhoneNumber(PhoneFormat);
        var request = new { displayName, phone };

        // Act
        var response = await client.PutAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();

        var customers = await GetCustomersByUserIdAsync(userId);
        var customer = customers.ShouldHaveSingleItem();
        customer.Id.ShouldBe(seeded.Id);
        customer.DisplayName.ShouldBe(displayName);
        customer.Phone.ShouldBe(phone);
    }

    [Fact]
    public async Task Put_WhenCustomerHasNoProfile_ShouldReturn204AndCreateProfileWithSubmittedValues()
    {
        await ResetDatabaseAsync();

        // Arrange
        var authName = _faker.Name.FullName();
        var userId = await SeedAuthUserAsync(authName);
        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);
        var displayName = $"{authName} {_faker.Random.AlphaNumeric(8)}";
        var phone = _faker.Phone.PhoneNumber(PhoneFormat);
        var request = new { displayName, phone };

        // Act
        var response = await client.PutAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).ShouldBeEmpty();

        var customers = await GetCustomersByUserIdAsync(userId);
        var customer = customers.ShouldHaveSingleItem();
        customer.DisplayName.ShouldBe(displayName);
        customer.Phone.ShouldBe(phone);
    }

    [Fact]
    public async Task Put_WhenPhoneIsEmpty_ShouldReturn204AndStorePhoneAsNull()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        await SeedCustomerAsync(userId);
        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);
        var request = new { displayName = _faker.Name.FullName(), phone = "" };

        // Act
        var response = await client.PutAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var customers = await GetCustomersByUserIdAsync(userId);
        customers.ShouldHaveSingleItem().Phone.ShouldBeNull();
    }

    [Fact]
    public async Task Put_WhenDisplayNameIsEmpty_ShouldReturn400WithValidationProblemDetailsAndKeepStoredValues()
    {
        await ResetDatabaseAsync();

        // Arrange
        var userId = await SeedAuthUserAsync(_faker.Name.FullName());
        var seeded = await SeedCustomerAsync(userId);
        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);
        var request = new { displayName = "", phone = _faker.Phone.PhoneNumber(PhoneFormat) };

        // Act
        var response = await client.PutAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Status.ShouldBe(400);
        body.Errors.ShouldContainKey("DisplayName");

        var customers = await GetCustomersByUserIdAsync(userId);
        var customer = customers.ShouldHaveSingleItem();
        customer.DisplayName.ShouldBe(seeded.DisplayName);
        customer.Phone.ShouldBe(seeded.Phone);
    }

    [Fact]
    public async Task Put_WhenDisplayNameChanges_ShouldKeepAuthUserName()
    {
        await ResetDatabaseAsync();

        // Arrange
        var authName = _faker.Name.FullName();
        var userId = await SeedAuthUserAsync(authName);
        using var client = CreateCustomerClient(userId, CustomersPermissions.ManageProfile);
        var displayName = $"{authName} {_faker.Random.AlphaNumeric(8)}";
        var request = new { displayName, phone = (string?)null };

        // Act
        var response = await client.PutAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var customers = await GetCustomersByUserIdAsync(userId);
        customers.ShouldHaveSingleItem().DisplayName.ShouldBe(displayName);

        using var scope = CreateScope();
        var authModule = scope.ServiceProvider.GetRequiredService<IAuthModule>();
        var user = await authModule.ExecuteQueryAsync(new GetUserByIdQuery(userId));
        user.Name.ShouldBe(authName);
    }

    private async Task<Customer> SeedCustomerAsync(int userId)
    {
        var storedName = _faker.Name.FullName();
        var customer = new Customer(userId, storedName);
        customer.UpdateProfile(storedName, _faker.Phone.PhoneNumber(PhoneFormat));
        await SeedAsync(db =>
        {
            db.Customers.Add(customer);
            return Task.CompletedTask;
        });
        return customer;
    }

    private async Task<List<Customer>> GetCustomersByUserIdAsync(int userId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        return await db.Customers.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
    }
}
