using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.SetUserStatus;

public sealed record SetUserStatusCommand(int Id, bool IsActive) : ICommand;
