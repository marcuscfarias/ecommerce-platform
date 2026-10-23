namespace Ecommerce.Customers.Application.Profiles.GetMyProfile;

public sealed record GetMyProfileResult(
    string DisplayName,
    string? Phone);
