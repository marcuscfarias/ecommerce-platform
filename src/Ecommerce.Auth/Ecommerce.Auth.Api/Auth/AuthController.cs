using Ecommerce.Auth.Api.Auth.Login;
using Ecommerce.Auth.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.Api.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthModule module) : ControllerBase
{
    [HttpPost("login")]
    [EndpointDescription("Authenticates a user and issues a JWT access token.")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return Ok(LoginResponse.FromResult(result));
    }
}
