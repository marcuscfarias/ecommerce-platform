using Ecommerce.Catalog.Api.Storefront.Categories.ListStorefrontCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Storefront.Categories;

[ApiController]
[Route("api/v1/storefront/categories")]
[AllowAnonymous]
public sealed class StorefrontCategoriesController : ControllerBase
{
    [HttpGet]
    [EndpointDescription("Returns the categories available in the storefront.")]
    [ProducesResponseType<IReadOnlyList<ListStorefrontCategoriesItemResponse>>(StatusCodes.Status200OK)]
    public IActionResult List(CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
