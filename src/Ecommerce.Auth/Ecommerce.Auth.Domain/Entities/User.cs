using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Auth.Domain.Entities;

public sealed class User(
    string email,
    string passwordHash,
    string firstName,
    string lastName,
    bool isActive = true) : Entity
{
    public string Email { get; private set; } = email;
    public string PasswordHash { get; private set; } = passwordHash;
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString("N");
    public string FirstName { get; private set; } = firstName;
    public string LastName { get; private set; } = lastName;
    public bool IsActive { get; private set; } = isActive;

    public void UpdateProfile(string firstName, string lastName, bool isActive)
    {
        FirstName = firstName;
        LastName = lastName;
        IsActive = isActive;
    }
}
