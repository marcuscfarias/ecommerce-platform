using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.DeleteUser;

public sealed record DeleteUserCommand(int Id) : ICommand;
