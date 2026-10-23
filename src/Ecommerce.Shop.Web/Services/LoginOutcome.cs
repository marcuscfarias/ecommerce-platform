namespace Ecommerce.Shop.Web.Services;

public sealed record LoginOutcome(bool Succeeded, string? Error, bool EmailNotConfirmed)
{
    public static LoginOutcome Success() => new(true, null, false);

    public static LoginOutcome Failure(string error) => new(false, error, false);

    public static LoginOutcome Unconfirmed(string error) => new(false, error, true);
}
