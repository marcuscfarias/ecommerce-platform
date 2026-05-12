using Ecommerce.Auth.Api.Users.CreateUser;
using Ecommerce.Auth.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.Api.Users;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(IAuthModule module) : ControllerBase
{
    [HttpPost]
    [EndpointDescription("Creates a new user.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var id = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return Created($"/api/v1/users/{id}", null);
    }
}
