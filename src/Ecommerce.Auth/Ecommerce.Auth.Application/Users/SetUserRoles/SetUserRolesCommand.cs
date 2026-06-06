using Ecommerce.Auth.Domain.Enums;
using Ecommerce.Kernel.Application.CQRS;

namespace Ecommerce.Auth.Application.Users.SetUserRoles;

public sealed record SetUserRolesCommand(int Id, IReadOnlyCollection<RoleName> Roles) : ICommand;
