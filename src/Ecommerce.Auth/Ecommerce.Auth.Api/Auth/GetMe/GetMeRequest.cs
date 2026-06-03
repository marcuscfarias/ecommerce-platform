using Ecommerce.Auth.Application.Auth.GetMe;

namespace Ecommerce.Auth.Api.Auth.GetMe;

public sealed record GetMeRequest
{
    internal static GetMeQuery ToQuery() => new();
}
