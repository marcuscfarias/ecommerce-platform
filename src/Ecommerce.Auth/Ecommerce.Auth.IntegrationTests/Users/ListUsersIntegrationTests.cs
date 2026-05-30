using Ecommerce.Auth.Api.Authorization;
using Ecommerce.Auth.Api.Users.ListUsers;
using Ecommerce.Auth.Domain.Entities;
using Ecommerce.Auth.IntegrationTests.Base;

namespace Ecommerce.Auth.IntegrationTests.Users;

public sealed class ListUsersIntegrationTests : BaseAuthIntegrationTest
{
    private const string Endpoint = "/api/v1/users";
    private readonly HttpClient _client;

    public ListUsersIntegrationTests(AuthIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(AuthPermissions.ViewUsers);

    [Fact]
    public async Task Get_WhenUsersExist_ShouldReturn200WithPagedResponse()
    {
        await ResetDatabaseAsync();

        // Arrange
        await SeedAsync(db =>
        {
            db.Users.Add(new User("alice@example.com", "hash", "Alice Smith"));
            db.Users.Add(new User("bob@example.com", "hash", "Bob Jones"));
            db.Users.Add(new User("carol@example.com", "hash", "Carol White"));
            return Task.CompletedTask;
        });

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListUsersResponse>();
        body.ShouldNotBeNull();
        body.Data.Count.ShouldBe(3);
        body.TotalCount.ShouldBe(3);
        body.Page.ShouldBe(1);
        body.TotalPages.ShouldBeGreaterThanOrEqualTo(1);

        var first = body.Data[0];
        first.Id.ShouldBeGreaterThan(0);
        first.Name.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_WhenNoUsersExist_ShouldReturn200WithEmptyResponse()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ListUsersResponse>();
        body.ShouldNotBeNull();
        body.Data.ShouldBeEmpty();
        body.TotalCount.ShouldBe(0);
    }
}
