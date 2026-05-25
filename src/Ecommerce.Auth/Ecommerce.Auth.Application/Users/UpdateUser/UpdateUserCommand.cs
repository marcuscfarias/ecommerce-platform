using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.UpdateUser;

public sealed record UpdateUserCommand(int Id, string Name) : ICommand;
