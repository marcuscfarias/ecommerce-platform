using Ecommerce.Catalog.Api.Storefront.Products.GetStorefrontProductById;
using Ecommerce.Catalog.Api.Storefront.Products.ListStorefrontProducts;
using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Storefront.Products;

[ApiController]
[Route("api/v1/storefront/products")]
[AllowAnonymous]
public sealed class StorefrontProductsController(ICatalogModule module) : ControllerBase
{
    [HttpGet]
    [EndpointDescription("Returns a paginated list of products on sale.")]
    [ProducesResponseType<ListStorefrontProductsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] ListStorefrontProductsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(request.ToQuery(), cancellationToken);
        return Ok(ListStorefrontProductsResponse.FromResult(result));
    }

    [HttpGet("{id:int}")]
    [EndpointDescription("Returns a product on sale by its ID.")]
    [ProducesResponseType<GetStorefrontProductByIdResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(GetStorefrontProductByIdRequest.ToQuery(id), cancellationToken);
        return Ok(GetStorefrontProductByIdResponse.FromResult(result));
    }

    [HttpGet("{id:int}/image")]
    [EndpointDescription("Returns the image of a product on sale.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult GetImage(
        [FromRoute] int id,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
