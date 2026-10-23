using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Customers.Domain.Entities;

public sealed class Customer(int userId, string displayName) : Entity
{
    public int UserId { get; private set; } = userId;
    public string DisplayName { get; private set; } = displayName;
    public string? Phone { get; private set; }

    public void UpdateProfile(string displayName, string? phone)
    {
        DisplayName = displayName;
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
    }
}
