namespace Ecommerce.Admin.Web.Services;

public sealed record UserOutcome(bool Succeeded, string? Error)
{
    public static UserOutcome Success() => new(true, null);

    public static UserOutcome Failure(string error) => new(false, error);
}
