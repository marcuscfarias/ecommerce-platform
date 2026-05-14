using Ecommerce.Auth.Application.Users.GetUserById;

namespace Ecommerce.Auth.Api.Users.GetUserById;

public sealed record GetUserByIdRequest
{
    internal static GetUserByIdQuery ToQuery(int id) => new(id);
}
