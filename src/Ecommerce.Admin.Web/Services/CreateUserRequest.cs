namespace Ecommerce.Admin.Web.Services;

public sealed record CreateUserRequest(string Email, string Password, string Name);
