using Ecommerce.Auth.Api.Users.CreateUser;
using Ecommerce.Auth.Api.Users.DeleteUser;
using Ecommerce.Auth.Api.Users.GetUserById;
using Ecommerce.Auth.Api.Users.ListUsers;
using Ecommerce.Auth.Api.Users.UpdateUser;
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
    public async Task<IActionResult> List(
        [FromQuery] ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(request.ToQuery(), cancellationToken);
        return Ok(ListUsersResponse.FromResult(result));
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
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPut("{id:int}")]
    [EndpointDescription("Updates an existing user.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(request.ToCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [EndpointDescription("Deletes a user by their ID.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
