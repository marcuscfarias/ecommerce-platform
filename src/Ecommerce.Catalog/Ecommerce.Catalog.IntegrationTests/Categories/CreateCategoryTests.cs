namespace Ecommerce.Catalog.IntegrationTests.Categories;

public sealed class CreateCategoryTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/categories";

    [Fact]
    public async Task Post_WhenRequestIsValid_Returns201WithLocationHeader()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            name = "Electronics",
            description = "Electronic devices and accessories"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldContain("/api/categories");
    }

    [Fact]
    public async Task Post_WhenDescriptionIsNull_Returns201()
    {
        await ResetDatabaseAsync();

        var response = await Client.PostAsJsonAsync(Endpoint, new
        {
            name = "Books",
            description = (string?)null
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_WhenNameAlreadyExists_Returns409()
    {
        await ResetDatabaseAsync();

        await Client.PostAsJsonAsync(Endpoint, new { name = "Clothing", description = (string?)null });

        var response = await Client.PostAsJsonAsync(Endpoint, new { name = "Clothing", description = (string?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public async Task Post_WhenNameIsInvalid_Returns400(string name)
    {
        var response = await Client.PostAsJsonAsync(Endpoint, new { name, description = (string?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
