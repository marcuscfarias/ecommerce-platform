using Ecommerce.Catalog.Api.Categories.CreateCategory;
using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Categories;

[ApiController]
[Route("api/v1/categories")]
public sealed class CategoriesController(ICatalogModule module) : ControllerBase
{
    [HttpPost]
    [EndpointDescription("Creates a new category.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var id = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet("{id:int}")]
    [EndpointDescription("Returns a category by its ID.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
