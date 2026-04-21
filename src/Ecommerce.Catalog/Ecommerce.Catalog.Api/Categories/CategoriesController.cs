using Ecommerce.Catalog.Api.Categories.CreateCategory;
using Ecommerce.Catalog.Api.Categories.GetCategoryById;
using Ecommerce.Catalog.Api.Categories.ListCategories;
using Ecommerce.Catalog.Api.Categories.UpdateCategory;
using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Categories;

[ApiController]
[Route("api/v1/categories")]
public sealed class CategoriesController(ICatalogModule module) : ControllerBase
{
    [HttpGet]
    [EndpointDescription("Returns a paginated list of categories.")]
    [ProducesResponseType<ListCategoriesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] ListCategoriesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(request.ToQuery(), cancellationToken);
        return Ok(ListCategoriesResponse.FromResult(result));
    }

    [HttpPost]
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
    [EndpointDescription("Returns a category by its ID.")]
    [ProducesResponseType<GetCategoryByIdResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(new GetCategoryByIdRequest().ToQuery(id), cancellationToken);
        return Ok(GetCategoryByIdResponse.FromResult(result));
    }

    [HttpPut("{id:int}")]
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
}