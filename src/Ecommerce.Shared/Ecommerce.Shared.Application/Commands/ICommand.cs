using MediatR;

namespace Ecommerce.Shared.Application.Commands;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
