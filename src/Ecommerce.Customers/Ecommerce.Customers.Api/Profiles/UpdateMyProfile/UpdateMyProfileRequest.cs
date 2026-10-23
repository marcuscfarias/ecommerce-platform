using Ecommerce.Customers.Application.Profiles.UpdateMyProfile;

namespace Ecommerce.Customers.Api.Profiles.UpdateMyProfile;

public sealed record UpdateMyProfileRequest(string DisplayName, string? Phone)
{
    internal UpdateMyProfileCommand ToCommand() => new(DisplayName, Phone);
}
