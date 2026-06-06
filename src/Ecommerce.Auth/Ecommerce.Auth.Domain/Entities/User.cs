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
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString("N");
    public int AccessFailedCount { get; private set; }
    public DateTimeOffset? LockoutEnd { get; private set; }

    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    public void UpdateProfile(string name) => Name = name;

    public void Deactivate()
    {
        IsActive = false;
        RotateSecurityStamp();
    }

    public void Reactivate() => IsActive = true;

    public void ResetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        RotateSecurityStamp();
    }

    public void RotateSecurityStamp() => SecurityStamp = Guid.NewGuid().ToString("N");

    public bool IsLockedOut(DateTimeOffset now) => LockoutEnd.HasValue && LockoutEnd.Value > now;

    public void RegisterFailedAccess(DateTimeOffset now, int maxAttempts, TimeSpan lockoutDuration)
    {
        AccessFailedCount++;

        if (AccessFailedCount < maxAttempts)
            return;

        LockoutEnd = now + lockoutDuration;
        AccessFailedCount = 0;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public void AssignRole(Role role)
    {
        if (_roles.Any(r => r.Id == role.Id))
            return;

        _roles.Add(role);
    }

    public void SetRoles(IEnumerable<Role> roles)
    {
        _roles.Clear();
        _roles.AddRange(roles.Distinct());
    }
}
