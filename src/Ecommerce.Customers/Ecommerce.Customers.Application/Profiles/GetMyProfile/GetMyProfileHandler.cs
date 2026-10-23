using MediatR;

namespace Ecommerce.Customers.Application.Profiles.GetMyProfile;

internal sealed class GetMyProfileHandler(CurrentCustomerProfile currentProfile)
    : IRequestHandler<GetMyProfileQuery, GetMyProfileResult>
{
    public async Task<GetMyProfileResult> Handle(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        var customer = await currentProfile.GetOrCreateAsync(cancellationToken);

        return new GetMyProfileResult(customer.DisplayName, customer.Phone);
    }
}
