namespace Ecommerce.Shop.Web.Services;

public sealed record CartOutcome(bool Succeeded, string? Error)
{
    public static CartOutcome Success() => new(true, null);

    public static CartOutcome Failure(string error) => new(false, error);
}
