using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Customers.Application.Profiles.GetMyProfile;

public sealed record GetMyProfileQuery : IQuery<GetMyProfileResult>;
