using MediatR;

namespace Ecommerce.Shared.Application.CQRS;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
