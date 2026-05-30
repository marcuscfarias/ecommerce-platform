namespace Ecommerce.Kernel.Application.Security;

// The claim type that carries authorization capabilities in the token.
// Shared, module-agnostic contract: Auth stamps it at issuance; every module's
// policies read it. Module-specific permission values live in each module.
public static class AppClaimTypes
{
    public const string Permission = "permission";
}
