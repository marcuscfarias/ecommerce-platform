namespace Ecommerce.Auth.Application.Auth.GetMe;

public sealed record GetMeResult(
    int Id,
    string Email,
    string Name,
    IReadOnlyCollection<string> Roles);
