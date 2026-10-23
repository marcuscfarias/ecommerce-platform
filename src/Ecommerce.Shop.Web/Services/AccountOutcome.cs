namespace Ecommerce.Shop.Web.Services;

public sealed record AccountOutcome(bool Succeeded, string? Error)
{
    public static AccountOutcome Success() => new(true, null);

    public static AccountOutcome Failure(string error) => new(false, error);
}
