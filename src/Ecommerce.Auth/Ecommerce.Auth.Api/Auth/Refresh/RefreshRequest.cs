using Ecommerce.Auth.Application.Auth.Refresh;

namespace Ecommerce.Auth.Api.Auth.Refresh;

public sealed record RefreshRequest
{
    internal static RefreshCommand ToCommand(string refreshToken) => new(refreshToken);
}
