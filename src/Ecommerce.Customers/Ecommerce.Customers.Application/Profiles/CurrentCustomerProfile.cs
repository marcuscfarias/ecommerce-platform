using Ecommerce.Auth.Application;
using Ecommerce.Auth.Application.Auth.GetMe;
using Ecommerce.Customers.Domain.Entities;
using Ecommerce.Customers.Domain.Repositories;
using Ecommerce.Kernel.Application.Security;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Customers.Application.Profiles;

internal sealed partial class CurrentCustomerProfile(
    ICustomerRepository repository,
    IUserContext userContext,
    IAuthModule authModule,
    ILogger<CurrentCustomerProfile> logger)
{
    public async Task<Customer> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var existing = await repository.GetByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
            return existing;

        var user = await authModule.ExecuteQueryAsync(new GetMeQuery(), cancellationToken);

        var customer = new Customer(userId, user.Name);
        repository.Add(customer);
        await repository.SaveChangesAsync(cancellationToken);

        LogProfileCreated(logger, customer.Id, userId);

        return customer;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Customer {CustomerId} profile created for user {UserId}")]
    private static partial void LogProfileCreated(ILogger logger, int customerId, int userId);
}
