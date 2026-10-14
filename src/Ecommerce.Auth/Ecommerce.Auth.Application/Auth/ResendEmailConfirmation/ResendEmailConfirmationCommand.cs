using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Auth.ResendEmailConfirmation;

public sealed record ResendEmailConfirmationCommand(string Email) : ICommand;
