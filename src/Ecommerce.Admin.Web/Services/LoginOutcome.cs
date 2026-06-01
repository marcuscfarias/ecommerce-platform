namespace Ecommerce.Admin.Web.Services;

public sealed record LoginOutcome(bool Succeeded, string? AccessToken, string? Error)
{
    public static LoginOutcome Success(string accessToken) => new(true, accessToken, null);

    public static LoginOutcome Failure(string error) => new(false, null, error);
}
