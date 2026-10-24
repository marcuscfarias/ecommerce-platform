using MediatR;

namespace Ecommerce.Customers.Application.Profiles.GetCurrentCustomerId;

internal sealed class GetCurrentCustomerIdHandler(CurrentCustomerProfile currentProfile)
    : IRequestHandler<GetCurrentCustomerIdQuery, int>
{
    public async Task<int> Handle(GetCurrentCustomerIdQuery query, CancellationToken cancellationToken)
    {
        var customer = await currentProfile.GetOrCreateAsync(cancellationToken);

        return customer.Id;
    }
}
