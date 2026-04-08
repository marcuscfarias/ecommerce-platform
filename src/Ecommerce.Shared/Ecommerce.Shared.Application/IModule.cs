using Ecommerce.Shared.Application.Commands;
using Ecommerce.Shared.Application.Queries;

namespace Ecommerce.Shared.Application;

public interface IModule
{
    Task ExecuteCommandAsync(ICommand command, CancellationToken cancellationToken = default);

    Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    Task<TResult> ExecuteQueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
