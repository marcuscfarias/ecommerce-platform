using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResult>;
