using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string Name) : ICommand<int>;
