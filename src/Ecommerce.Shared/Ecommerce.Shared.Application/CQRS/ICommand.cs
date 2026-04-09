using MediatR;

namespace Ecommerce.Shared.Application.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
