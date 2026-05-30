using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class CreateCategoryIntegrationTests : BaseCatalogIntegrationTest
{
    private const string Endpoint = "/api/v1/categories";
    private readonly HttpClient _client;

    public CreateCategoryIntegrationTests(CatalogIntegrationFixture fixture) : base(fixture) =>
        _client = CreateAuthenticatedClient(CatalogPermissions.Manage);

    [Fact]
    public async Task Post_WhenRequestIsValid_ShouldReturn201WithLocationHeader()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            name = "Electronics",
            description = "Electronic devices and accessories"
        };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var location = response.Headers.Location!.ToString();
        location.ShouldContain("/api/v1/categories/");

        var idSegment = location[(location.LastIndexOf('/') + 1)..];
        int.TryParse(idSegment, out var id).ShouldBeTrue();
        id.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Post_WhenNameIsEmpty_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { name = "", description = (string?)null };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Post_WhenDescriptionIsTooShort_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { name = "Electronics", description = "short" };

        // Act
        var response = await _client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldContainKey("Description");
    }
}
