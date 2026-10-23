using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Customers.Application.Profiles.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(string DisplayName, string? Phone) : ICommand;
