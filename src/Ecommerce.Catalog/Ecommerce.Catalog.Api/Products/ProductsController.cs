using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Api.Products.CreateProduct;
using Ecommerce.Catalog.Api.Products.GetProductById;
using Ecommerce.Catalog.Api.Products.ListProducts;
using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Products;

[ApiController]
[Route("api/v1/products")]
public sealed class ProductsController(ICatalogModule module) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = CatalogPolicies.CanViewCatalog)]
    [EndpointDescription("Returns a paginated list of products.")]
    [ProducesResponseType<ListProductsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListProductsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(request.ToQuery(), cancellationToken);
        return Ok(ListProductsResponse.FromResult(result));
    }

    [HttpPost]
    [Authorize(Policy = CatalogPolicies.CanManageCatalog)]
    [EndpointDescription("Creates a new product.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var id = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = CatalogPolicies.CanViewCatalog)]
    [EndpointDescription("Returns a product by its ID.")]
    [ProducesResponseType<GetProductByIdResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(GetProductByIdRequest.ToQuery(id), cancellationToken);
        return Ok(GetProductByIdResponse.FromResult(result));
    }
}
