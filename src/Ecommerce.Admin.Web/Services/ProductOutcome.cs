namespace Ecommerce.Admin.Web.Services;

public sealed record ProductOutcome(bool Succeeded, string? Error)
{
    public static ProductOutcome Success() => new(true, null);

    public static ProductOutcome Failure(string error) => new(false, error);
}
