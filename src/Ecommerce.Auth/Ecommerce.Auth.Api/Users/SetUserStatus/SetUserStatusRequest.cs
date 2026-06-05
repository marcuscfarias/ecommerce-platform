using Ecommerce.Auth.Application.Users.SetUserStatus;

namespace Ecommerce.Auth.Api.Users.SetUserStatus;

public sealed record SetUserStatusRequest(bool IsActive)
{
    internal SetUserStatusCommand ToCommand(int id) => new(id, IsActive);
}
