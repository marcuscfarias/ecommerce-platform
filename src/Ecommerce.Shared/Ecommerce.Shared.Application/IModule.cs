using Ecommerce.Shared.Application.Commands;

namespace Ecommerce.Shared.Application;

public interface IModule
{
    Task ExecuteCommandAsync(ICommand command, CancellationToken cancellationToken = default);

    Task<TResult> ExecuteCommandAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}
