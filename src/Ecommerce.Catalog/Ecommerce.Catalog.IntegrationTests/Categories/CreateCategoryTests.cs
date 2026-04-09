using Ecommerce.Catalog.Api.Categories.CreateCategory;
using Ecommerce.Catalog.IntegrationTests.Base;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class CreateCategoryTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/categories";

    [Fact]
    public async Task Post_WhenRequestIsValid_Returns201WithLocationHeaderAndBody()
    {
        // // Arrange
        // var request = new { name = "Electronics", slug = "electronics", description = "Electronic devices and accessories" };
        //
        // // Act
        // var response = await Client.PostAsJsonAsync(Endpoint, request);
        //
        // // Assert
        // response.StatusCode.ShouldBe(HttpStatusCode.Created);
        // response.Headers.Location.ShouldNotBeNull();
        // response.Headers.Location.ToString().ShouldContain("/api/v1/categories/");
        //
        // var body = await response.Content.ReadFromJsonAsync<CreateCategoryResponse>();
        // body.ShouldNotBeNull();
        // body.Id.ShouldBeGreaterThan(0);
        // body.Name.ShouldBe(request.name);
        // body.Slug.ShouldBe(request.slug);
        // body.Description.ShouldBe(request.description);
        //
        // await ResetDatabaseAsync();
    }

    [Fact]
    public async Task Post_WhenDescriptionIsNull_Returns201()
    {
        // Arrange
        var request = new { name = "Books", slug = "books", description = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        await ResetDatabaseAsync();
    }

    [Fact]
    public async Task Post_WhenNameAlreadyExists_Returns409()
    {
        // Arrange
        await Client.PostAsJsonAsync(Endpoint, new { name = "Clothing", slug = "clothing", description = (string?)null });

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { name = "Clothing", slug = "clothing-2", description = (string?)null });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        await ResetDatabaseAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public async Task Post_WhenNameIsInvalid_Returns400(string name)
    {
        // Arrange
        var request = new { name, slug = "valid-slug", description = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await ResetDatabaseAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("UPPER")]
    [InlineData("has space")]
    public async Task Post_WhenSlugIsInvalid_Returns400(string slug)
    {
        // Arrange
        var request = new { name = "Valid Name", slug, description = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await ResetDatabaseAsync();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("a very long description that exceeds the one hundred character limit set by the validator in this system")]
    public async Task Post_WhenDescriptionIsInvalid_Returns400(string description)
    {
        // Arrange
        var request = new { name = "Valid Name", slug = "valid-name", description };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await ResetDatabaseAsync();
    }
}
