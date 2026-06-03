using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Auth.GetMe;

public sealed record GetMeQuery : IQuery<GetMeResult>;
