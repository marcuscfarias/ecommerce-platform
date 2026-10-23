using Ecommerce.Customers.Domain.Repositories;
using MediatR;

namespace Ecommerce.Customers.Application.Profiles.UpdateMyProfile;

internal sealed class UpdateMyProfileHandler(CurrentCustomerProfile currentProfile, ICustomerRepository repository)
    : IRequestHandler<UpdateMyProfileCommand>
{
    public async Task Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        var customer = await currentProfile.GetOrCreateAsync(cancellationToken);

        customer.UpdateProfile(command.DisplayName, command.Phone);
        repository.Update(customer);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
