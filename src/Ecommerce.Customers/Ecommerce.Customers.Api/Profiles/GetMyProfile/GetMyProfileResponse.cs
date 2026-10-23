using Ecommerce.Customers.Application.Profiles.GetMyProfile;

namespace Ecommerce.Customers.Api.Profiles.GetMyProfile;

public sealed record GetMyProfileResponse(
    string DisplayName,
    string? Phone)
{
    internal static GetMyProfileResponse FromResult(GetMyProfileResult result) =>
        new(result.DisplayName, result.Phone);
}
