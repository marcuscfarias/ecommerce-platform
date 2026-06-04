namespace Ecommerce.Admin.Web.Services;

public sealed record LoginOutcome(bool Succeeded, string? Error)
{
    public static LoginOutcome Success() => new(true, null);

    public static LoginOutcome Failure(string error) => new(false, error);
}
