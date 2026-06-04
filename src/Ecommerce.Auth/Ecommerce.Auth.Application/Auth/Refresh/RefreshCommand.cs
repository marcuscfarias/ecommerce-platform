using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Auth.Refresh;

public sealed record RefreshCommand(string RefreshToken) : ICommand<RefreshResult>;
