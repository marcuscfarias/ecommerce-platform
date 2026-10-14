namespace Ecommerce.Auth.Application.Auth.Security;

public sealed record EmailConfirmationTokenPair(string Plain, string Hash);
