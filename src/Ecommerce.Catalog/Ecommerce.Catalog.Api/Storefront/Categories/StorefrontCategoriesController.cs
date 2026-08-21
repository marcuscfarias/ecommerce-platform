using Ecommerce.Catalog.Api.Storefront.Categories.ListStorefrontCategories;
using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Storefront.Categories;

[ApiController]
[Route("api/v1/storefront/categories")]
[AllowAnonymous]
public sealed class StorefrontCategoriesController(ICatalogModule module) : ControllerBase
{
    [HttpGet]
    [EndpointDescription("Returns the categories available in the storefront.")]
    [ProducesResponseType<IReadOnlyList<ListStorefrontCategoriesItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(ListStorefrontCategoriesRequest.ToQuery(), cancellationToken);
        return Ok(ListStorefrontCategoriesItemResponse.FromResult(result));
    }
}
