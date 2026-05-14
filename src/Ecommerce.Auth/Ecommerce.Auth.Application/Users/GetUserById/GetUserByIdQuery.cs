using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.GetUserById;

public sealed record GetUserByIdQuery(int Id) : IQuery<GetUserByIdResult>;
