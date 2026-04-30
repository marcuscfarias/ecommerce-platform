using MediatR;

namespace Ecommerce.Kernel.Application.CQRS;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
