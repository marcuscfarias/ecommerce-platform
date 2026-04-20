using Ecommerce.Catalog.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class CreateCategoryIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/categories";

    [Fact]
    public async Task Post_WhenRequestIsValid_ShouldReturn201WithLocationHeader()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new
        {
            name = "Electronics",
            slug = "electronics",
            description = "Electronic devices and accessories"
        };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

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
    public async Task Post_WhenRequestIsInvalid_ShouldReturn400WithValidationProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var request = new { name = "Valid Name", slug = "", description = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        
        var body = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        body.ShouldNotBeNull();
        body.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Post_WhenSlugAlreadyExists_ShouldReturn409WithProblemDetails()
    {
        await ResetDatabaseAsync();

        // Arrange
        var existing = new { name = "Electronics", slug = "electronics", description = (string?)null };
        var firstResponse = await Client.PostAsJsonAsync(Endpoint, existing);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var duplicate = new { name = "Other Name", slug = "electronics", description = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, duplicate);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body.ShouldNotBeNull();
    }
}
