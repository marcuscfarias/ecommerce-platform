using Ecommerce.Catalog.Api.Authorization;
using Ecommerce.Catalog.Api.Categories.CreateCategory;
using Ecommerce.Catalog.Api.Categories.DeleteCategory;
using Ecommerce.Catalog.Api.Categories.GetCategoryById;
using Ecommerce.Catalog.Api.Categories.ListCategories;
using Ecommerce.Catalog.Api.Categories.UpdateCategory;
using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Categories;

[ApiController]
[Route("api/v1/categories")]
public sealed class CategoriesController(ICatalogModule module) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = CatalogPolicies.CanViewCatalog)]
    [EndpointDescription("Returns a paginated list of categories.")]
    [ProducesResponseType<ListCategoriesResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] ListCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(request.ToQuery(), cancellationToken);
        return Ok(ListCategoriesResponse.FromResult(result));
    }

    [HttpPost]
    [Authorize(Policy = CatalogPolicies.CanManageCatalog)]
    [EndpointDescription("Creates a new category.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var id = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = CatalogPolicies.CanViewCatalog)]
    [EndpointDescription("Returns a category by its ID.")]
    [ProducesResponseType<GetCategoryByIdResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(GetCategoryByIdRequest.ToQuery(id), cancellationToken);
        return Ok(GetCategoryByIdResponse.FromResult(result));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = CatalogPolicies.CanManageCatalog)]
    [EndpointDescription("Updates an existing category.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(request.ToCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = CatalogPolicies.CanManageCatalog)]
    [EndpointDescription("Deletes a category by its ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(DeleteCategoryRequest.ToCommand(id), cancellationToken);
        return NoContent();
    }
}
