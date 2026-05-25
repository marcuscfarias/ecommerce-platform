using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Auth.Domain.Entities;

public sealed class User(
    string email,
    string passwordHash,
    string name,
    bool isActive = true) : Entity
{
    public string Email { get; private set; } = email;
    public string PasswordHash { get; private set; } = passwordHash;
    public string Name { get; private set; } = name;
    public bool IsActive { get; private set; } = isActive;

    public void UpdateProfile(string name) => Name = name;

    public void Deactivate() => IsActive = false;
}
