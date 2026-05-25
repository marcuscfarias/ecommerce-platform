using Ecommerce.Auth.Application.Users.UpdateUser;

namespace Ecommerce.Auth.Api.Users.UpdateUser;

public sealed record UpdateUserRequest(string Name)
{
    internal UpdateUserCommand ToCommand(int id) => new(id, Name.Trim());
}
