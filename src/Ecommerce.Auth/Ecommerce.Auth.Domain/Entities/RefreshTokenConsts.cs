namespace Ecommerce.Auth.Domain.Entities;

public static class RefreshTokenConsts
{
    public const int TokenHashMaxLength = 64;
    public const int StampSnapshotMaxLength = UserConsts.SecurityStampMaxLength;
}
