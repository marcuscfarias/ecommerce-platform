using Ecommerce.Catalog.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalog.Api.Categories;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ICatalogModule module) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var id = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(CreateAsync), new { id }, null);
    }
}
