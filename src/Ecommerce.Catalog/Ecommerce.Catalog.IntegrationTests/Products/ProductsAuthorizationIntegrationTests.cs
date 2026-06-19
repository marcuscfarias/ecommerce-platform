using System.Net.Http.Headers;
using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.IntegrationTests.Base;

namespace Ecommerce.Catalog.IntegrationTests.Products;

public sealed class ProductsAuthorizationIntegrationTests(CatalogIntegrationFixture fixture)
    : BaseCatalogIntegrationTest(fixture)
{
    private const string Endpoint = "/api/v1/products";

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

        // Act
        var response = await Client.PostAsJsonAsync(Endpoint, ProductBody());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);

        // Act
        var response = await client.PostAsJsonAsync(Endpoint, ProductBody());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Put_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/1", ProductBody());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/1", ProductBody());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PutStatus_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"{Endpoint}/1/status", new { isActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PutStatus_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);

        // Act
        var response = await client.PutAsJsonAsync($"{Endpoint}/1/status", new { isActive = false });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostImage_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Arrange
        using var form = ImageForm();

        // Act
        var response = await Client.PostAsync($"{Endpoint}/1/image", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostImage_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);
        using var form = ImageForm();

        // Act
        var response = await client.PostAsync($"{Endpoint}/1/image", form);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteImage_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.DeleteAsync($"{Endpoint}/1/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteImage_WhenTokenHasViewButNotManage_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient(CatalogPermissions.View);

        // Act
        var response = await client.DeleteAsync($"{Endpoint}/1/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetImage_WhenNoToken_ShouldReturn401()
    {
        await ResetDatabaseAsync();

        // Act
        var response = await Client.GetAsync($"{Endpoint}/1/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetImage_WhenTokenLacksPermission_ShouldReturn403()
    {
        await ResetDatabaseAsync();

        // Arrange
        var client = CreateAuthenticatedClient("unrelated:permission");

        // Act
        var response = await client.GetAsync($"{Endpoint}/1/image");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static object ProductBody() => new
    {
        name = "Wireless Mouse",
        description = (string?)null,
        price = 29.99m,
        sku = "ELEC-WM-100",
        categoryId = 1,
        stockQuantity = 10,
    };

    private static MultipartFormDataContent ImageForm()
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[256]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(fileContent, "file", "image.jpg");
        return form;
    }
}
