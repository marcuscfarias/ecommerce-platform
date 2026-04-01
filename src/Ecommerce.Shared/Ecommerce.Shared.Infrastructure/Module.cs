using Ecommerce.Shared.Application;
using Ecommerce.Shared.Application.Commands;
using Ecommerce.Shared.Application.Queries;
using MediatR;

namespace Ecommerce.Shared.Infrastructure;

public class Module(ISender mediator) : IModule
{
    public async Task ExecuteCommandAsync(ICommand command, CancellationToken cancellationToken = default) =>
        await mediator.Send(command, cancellationToken);

    public async Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default) =>
        await mediator.Send(command, cancellationToken);

    public async Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default) =>
        await mediator.Send(query, cancellationToken);
}
