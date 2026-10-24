using Ecommerce.Orders.Api.Authorization;
using Ecommerce.Orders.Api.Carts.GetMyCart;
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
}
