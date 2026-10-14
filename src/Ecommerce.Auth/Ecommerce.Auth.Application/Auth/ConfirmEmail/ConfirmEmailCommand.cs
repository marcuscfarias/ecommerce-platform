using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Token) : ICommand<ConfirmEmailResult>;
