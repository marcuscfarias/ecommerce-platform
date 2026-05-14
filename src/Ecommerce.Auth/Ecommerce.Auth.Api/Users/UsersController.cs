using Ecommerce.Auth.Api.Users.CreateUser;
using Ecommerce.Auth.Api.Users.GetUserById;
using Ecommerce.Auth.Api.Users.ListUsers;
using Ecommerce.Auth.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.Api.Users;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(IAuthModule module) : ControllerBase
{
    [HttpGet]
    [EndpointDescription("Returns a paginated list of users.")]
    [ProducesResponseType<ListUsersResponse>(StatusCodes.Status200OK)]
    public Task<IActionResult> List(
        [FromQuery] ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

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

    [HttpGet("{id:int}")]
    [EndpointDescription("Returns a user by their ID.")]
    [ProducesResponseType<GetUserByIdResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(GetUserByIdRequest.ToQuery(id), cancellationToken);
        return Ok(GetUserByIdResponse.FromResult(result));
    }
}
