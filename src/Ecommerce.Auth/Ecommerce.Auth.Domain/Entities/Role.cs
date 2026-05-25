using Ecommerce.Kernel.Domain.Entities;

namespace Ecommerce.Auth.Domain.Entities;

public sealed class Role(string name) : Entity
{
    public string Name { get; private set; } = name;
}
