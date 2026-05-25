using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Auth.Domain.Entities;

public sealed class User(
    string email,
    string passwordHash,
    string name,
    bool isActive = true) : Entity
{
    private readonly List<Role> _roles = [];

    public string Email { get; private set; } = email;
    public string PasswordHash { get; private set; } = passwordHash;
    public string Name { get; private set; } = name;
    public bool IsActive { get; private set; } = isActive;
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public void UpdateProfile(string name) => Name = name;

    public void Deactivate() => IsActive = false;

    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
            return;

        _roles.Add(role);
    }
}
