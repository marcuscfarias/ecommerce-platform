using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResult>;
