using Ecommerce.Customers.Application.Profiles.GetMyProfile;

namespace Ecommerce.Customers.Api.Profiles.GetMyProfile;

public sealed record GetMyProfileRequest
{
    internal static GetMyProfileQuery ToQuery() => new();
}
