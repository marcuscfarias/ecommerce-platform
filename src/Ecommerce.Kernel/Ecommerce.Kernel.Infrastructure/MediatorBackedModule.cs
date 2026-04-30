using Ecommerce.Kernel.Application;
using Ecommerce.Kernel.Application.CQRS;
using MediatR;

namespace Ecommerce.Kernel.Infrastructure;

public class MediatorBackedModule(ISender mediator) : IModule
{
    public async Task ExecuteCommandAsync(ICommand command, CancellationToken cancellationToken = default) =>
        await mediator.Send(command, cancellationToken);

    public async Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default) =>
        await mediator.Send(command, cancellationToken);

    public async Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default) =>
        await mediator.Send(query, cancellationToken);
}
