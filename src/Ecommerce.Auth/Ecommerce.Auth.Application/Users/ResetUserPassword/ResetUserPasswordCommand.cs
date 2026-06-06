using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.ResetUserPassword;

public sealed record ResetUserPasswordCommand(int Id, string Password) : ICommand;
