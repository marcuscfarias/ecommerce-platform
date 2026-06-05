namespace Ecommerce.Admin.Web.Services;

public sealed record CategoryOutcome(bool Succeeded, string? Error)
{
    public static CategoryOutcome Success() => new(true, null);

    public static CategoryOutcome Failure(string error) => new(false, error);
}
