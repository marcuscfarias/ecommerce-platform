using Ecommerce.Auth.Application.Users.UpdateUser;

namespace Ecommerce.Auth.Api.Users.UpdateUser;

public sealed record UpdateUserRequest(string FirstName, string LastName, bool IsActive)
{
    internal UpdateUserCommand ToCommand(int id) => new(id, FirstName.Trim(), LastName.Trim(), IsActive);
}
