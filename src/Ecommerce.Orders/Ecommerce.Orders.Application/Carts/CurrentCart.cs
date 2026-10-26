using Ecommerce.Kernel.Application.Security;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Orders.Application.Carts;

internal sealed partial class CurrentCart(
    ICartRepository repository,
    IUserContext userContext,
    ILogger<CurrentCart> logger)
{
    private Cart? _created;

    public Task<Cart?> FindAsync(CancellationToken cancellationToken) =>
        repository.GetByUserIdAsync(userContext.UserId, cancellationToken);

    public async Task<Cart> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var existing = await repository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
            return existing;

        _created = new Cart(userId);
        repository.Add(_created);

        return _created;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await repository.SaveChangesAsync(cancellationToken);

        if (_created is null)
            return;

        LogCartCreated(logger, _created.Id, _created.UserId);
        _created = null;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Cart {CartId} created for user {UserId}")]
    private static partial void LogCartCreated(ILogger logger, int cartId, int userId);
}
