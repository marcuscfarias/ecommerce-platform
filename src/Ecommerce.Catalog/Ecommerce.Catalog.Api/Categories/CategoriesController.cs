using Ecommerce.Catalog.Api.Categories.CreateCategory;
using Ecommerce.Catalog.Api.Categories.GetCategories;
using Ecommerce.Catalog.Api.Categories.GetCategoryById;
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
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [HttpGet]
    [EndpointDescription("Returns all categories.")]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    [HttpGet("{id:int}")]
    [EndpointDescription("Returns a category by its ID.")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
