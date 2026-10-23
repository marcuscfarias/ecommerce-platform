using Ecommerce.Customers.Api.Authorization;
using Ecommerce.Customers.Api.Profiles.GetMyProfile;
using Ecommerce.Customers.Api.Profiles.UpdateMyProfile;
using Ecommerce.Customers.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Customers.Api.Profiles;

[ApiController]
[Route("api/v1/customers")]
[Authorize(Policy = CustomersPolicies.CanManageProfile)]
public sealed class CustomersController(ICustomerModule module) : ControllerBase
{
    [HttpGet("me")]
    [EndpointDescription("Returns the authenticated customer's profile, creating it from the account name on first access.")]
    [ProducesResponseType<GetMyProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(GetMyProfileRequest.ToQuery(), cancellationToken);
        return Ok(GetMyProfileResponse.FromResult(result));
    }

    [HttpPut("me")]
    [EndpointDescription("Replaces the authenticated customer's display name and phone, creating the profile if it does not exist.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return NoContent();
    }
}
