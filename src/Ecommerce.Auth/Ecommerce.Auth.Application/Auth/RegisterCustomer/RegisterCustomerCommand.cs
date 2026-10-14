using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Auth.RegisterCustomer;

public sealed record RegisterCustomerCommand(
    string Name,
    string Email,
    string Password) : ICommand;
