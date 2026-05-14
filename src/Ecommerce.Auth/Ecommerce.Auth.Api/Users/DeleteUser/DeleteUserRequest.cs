using Ecommerce.Auth.Application.Users.DeleteUser;

namespace Ecommerce.Auth.Api.Users.DeleteUser;

public sealed record DeleteUserRequest
{
    internal static DeleteUserCommand ToCommand(int id) => new(id);
}
