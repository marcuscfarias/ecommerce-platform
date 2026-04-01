using MediatR;

namespace Ecommerce.Shared.Application.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
