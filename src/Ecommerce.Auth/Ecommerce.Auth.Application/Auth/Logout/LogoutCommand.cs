using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;
