using Ecommerce.Orders.Api.Authorization;
using Ecommerce.Orders.Api.Carts.AddCartItem;
using Ecommerce.Orders.Api.Carts.ChangeCartItemQuantity;
using Ecommerce.Orders.Api.Carts.GetMyCart;
using Ecommerce.Orders.Api.Carts.RemoveCartItem;
using Ecommerce.Orders.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Orders.Api.Carts;

[ApiController]
[Route("api/v1/carts")]
[Authorize(Policy = OrdersPolicies.CanManageCart)]
public sealed class CartsController(IOrderModule module) : ControllerBase
{
    [HttpGet("me")]
    [EndpointDescription("Returns the authenticated customer's cart with current catalog data; an empty cart when none exists.")]
    [ProducesResponseType<GetMyCartResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyCart(CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(GetMyCartRequest.ToQuery(), cancellationToken);
        return Ok(GetMyCartResponse.FromResult(result));
    }

    [HttpPost("me/items")]
    [EndpointDescription("Adds a quantity of a product to the cart, summing into the existing line; creates the cart on first add.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddCartItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return NoContent();
    }

    [HttpPut("me/items/{productId:int}")]
    [EndpointDescription("Replaces the quantity of a product already in the cart.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeCartItemQuantity(
        int productId,
        [FromBody] ChangeCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(request.ToCommand(productId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("me/items/{productId:int}")]
    [EndpointDescription("Removes a product's line from the cart, whatever its status.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCartItem(int productId, CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(RemoveCartItemRequest.ToCommand(productId), cancellationToken);
        return NoContent();
    }
}
