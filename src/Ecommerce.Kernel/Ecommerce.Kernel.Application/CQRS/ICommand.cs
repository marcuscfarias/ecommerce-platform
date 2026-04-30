using MediatR;

namespace Ecommerce.Kernel.Application.CQRS;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
