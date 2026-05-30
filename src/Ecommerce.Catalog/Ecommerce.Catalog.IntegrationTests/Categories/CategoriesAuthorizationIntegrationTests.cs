using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.IntegrationTests.Base;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class CategoriesAuthorizationIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/categories";

    [Fact]
    public async Task Get_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_WhenTokenLacksPermission_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient("unrelated:permission");

        // Act
        var response = await client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { name = "Electronics", description = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);
        var request = new { name = "Electronics", description = (string?)null };

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Put_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);
        var request = new { name = "Electronics", description = (string?)null };

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/1", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);

        // Act
        var response = await client.DeleteAsync($"{Endpoint}/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
