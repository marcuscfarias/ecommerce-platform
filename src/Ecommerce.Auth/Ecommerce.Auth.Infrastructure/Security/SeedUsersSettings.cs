using Ecommerce.Auth.Domain.Enums;

namespace Ecommerce.Auth.Infrastructure.Security;

public sealed class SeedUsersSettings
{
    public List<SeedUserSettings> Users { get; init; } = [];
}

public sealed class SeedUserSettings
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public RoleName Role { get; init; }
}
