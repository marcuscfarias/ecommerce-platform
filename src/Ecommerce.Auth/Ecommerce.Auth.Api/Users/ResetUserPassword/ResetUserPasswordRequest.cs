using Ecommerce.Auth.Application.Users.ResetUserPassword;

namespace Ecommerce.Auth.Api.Users.ResetUserPassword;

public sealed record ResetUserPasswordRequest(string Password)
{
    internal ResetUserPasswordCommand ToCommand(int id) => new(id, Password);
}
