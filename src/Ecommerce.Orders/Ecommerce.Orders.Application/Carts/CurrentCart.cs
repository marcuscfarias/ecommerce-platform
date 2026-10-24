using Ecommerce.Customers.Application;
using Ecommerce.Customers.Application.Profiles.GetCurrentCustomerId;
using Ecommerce.Orders.Domain.Entities;
using Ecommerce.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Orders.Application.Carts;

internal sealed partial class CurrentCart(
    ICartRepository repository,
    ICustomerModule customerModule,
    ILogger<CurrentCart> logger)
{
    private Cart? _created;

    public async Task<Cart?> FindAsync(CancellationToken cancellationToken)
    {
        var customerId = await GetCustomerIdAsync(cancellationToken);

        return await repository.GetByCustomerIdAsync(customerId, cancellationToken);
    }

    public async Task<Cart> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var customerId = await GetCustomerIdAsync(cancellationToken);

        var existing = await repository.GetByCustomerIdAsync(customerId, cancellationToken);
        if (existing is not null)
            return existing;

        _created = new Cart(customerId);
        repository.Add(_created);

        return _created;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await repository.SaveChangesAsync(cancellationToken);

        if (_created is null)
            return;

        LogCartCreated(logger, _created.Id, _created.CustomerId);
        _created = null;
    }

    private Task<int> GetCustomerIdAsync(CancellationToken cancellationToken) =>
        customerModule.ExecuteQueryAsync(new GetCurrentCustomerIdQuery(), cancellationToken);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Cart {CartId} created for customer {CustomerId}")]
    private static partial void LogCartCreated(ILogger logger, int cartId, int customerId);
}
