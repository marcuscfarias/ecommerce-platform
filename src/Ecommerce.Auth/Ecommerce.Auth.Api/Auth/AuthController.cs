using Ecommerce.Auth.Api.Auth.GetMe;
using Ecommerce.Auth.Api.Auth.Login;
using Ecommerce.Auth.Api.Auth.Refresh;
using Ecommerce.Auth.Application;
using Ecommerce.Kernel.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.Api.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthModule module) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [EndpointDescription("Authenticates a user, sets the access/refresh cookies, and returns the user summary.")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);

        Response.SetAuthCookies(result.Tokens);

        return Ok(LoginResponse.FromResult(result));
    }

    [Authorize]
    [HttpGet("me")]
    [EndpointDescription("Returns the authenticated user's identity.")]
    [ProducesResponseType<GetMeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var result = await module.ExecuteQueryAsync(GetMeRequest.ToQuery(), cancellationToken);
        return Ok(GetMeResponse.FromResult(result));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [EndpointDescription("Issues a new access token from a valid refresh cookie and re-sets the access cookie.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken] ?? string.Empty;

        var result = await module.ExecuteCommandAsync(RefreshRequest.ToCommand(refreshToken), cancellationToken);

        Response.SetAccessCookie(result.AccessToken, result.AccessTokenExpiresInSeconds);

        return NoContent();
    }
}
