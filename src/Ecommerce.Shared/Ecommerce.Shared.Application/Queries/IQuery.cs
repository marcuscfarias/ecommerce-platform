using MediatR;

namespace Ecommerce.Shared.Application.Queries;

public interface IQuery<out TResult> : IRequest<TResult>
{
}
