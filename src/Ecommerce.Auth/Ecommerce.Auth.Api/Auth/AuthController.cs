using Ecommerce.Auth.Api.Auth.ConfirmEmail;
using Ecommerce.Auth.Api.Auth.GetMe;
using Ecommerce.Auth.Api.Auth.Login;
using Ecommerce.Auth.Api.Auth.Logout;
using Ecommerce.Auth.Api.Auth.Refresh;
using Ecommerce.Auth.Api.Auth.Register;
using Ecommerce.Auth.Api.Auth.ResendConfirmation;
using Ecommerce.Auth.Application;
using Ecommerce.Kernel.API.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecommerce.Auth.Api.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthModule module) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingPolicies.Login)]
    [HttpPost("login")]
    [EndpointDescription("Authenticates a user, sets the access/refresh cookies, and returns the user summary.")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);

        Response.SetAuthCookies(result.Tokens);

        return Ok(LoginResponse.FromResult(result));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [EndpointDescription("Registers a customer account and emails a confirmation link. Responds 202 whether or not the email is already registered.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return Accepted();
    }

    [AllowAnonymous]
    [HttpGet("confirm-email")]
    [EndpointDescription("Confirms the email from the emailed link and redirects to the shop with the outcome (success or invalid).")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        var result = await module.ExecuteCommandAsync(ConfirmEmailRequest.ToCommand(token), cancellationToken);
        return Redirect(result.RedirectTo.AbsoluteUri);
    }

    [AllowAnonymous]
    [HttpPost("resend-confirmation")]
    [EndpointDescription("Emails a new confirmation link to a pending account. Responds 202 whether or not the email is registered or confirmed.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendEmailConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        await module.ExecuteCommandAsync(request.ToCommand(), cancellationToken);
        return Accepted();
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

    [AllowAnonymous]
    [HttpPost("logout")]
    [EndpointDescription("Revokes the presented refresh token and clears the access/refresh cookies.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken] ?? string.Empty;

        await module.ExecuteCommandAsync(LogoutRequest.ToCommand(refreshToken), cancellationToken);

        Response.ClearAuthCookies();

        return NoContent();
    }
}
