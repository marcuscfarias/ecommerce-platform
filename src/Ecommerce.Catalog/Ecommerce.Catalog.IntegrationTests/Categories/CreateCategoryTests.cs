using Ecommerce.Catalog.IntegrationTests.Base;

namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class CreateCategoryTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/categories";

    [Fact]
    public async Task Post_WhenRequestIsValid_Returns201WithLocationHeader()
    {
        // Arrange
        var request = new { name = "Electronics", description = "Electronic devices and accessories" };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldContain("/api/categories");

        await ResetDatabaseAsync();
    }

    [Fact]
    public async Task Post_WhenDescriptionIsNull_Returns201()
    {
        // Arrange
        var request = new { name = "Books", description = (string?)null };

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
        await Client.PostAsJsonAsync(Endpoint, new { name = "Clothing", description = (string?)null });

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, new { name = "Clothing", description = (string?)null });

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
        var request = new { name, description = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await ResetDatabaseAsync();
    }
}
