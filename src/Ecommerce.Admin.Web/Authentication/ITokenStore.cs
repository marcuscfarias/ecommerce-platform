namespace Ecommerce.Admin.Web.Authentication;

public interface ITokenStore
{
    ValueTask<string?> GetTokenAsync();

    ValueTask SetTokenAsync(string token);

    ValueTask ClearAsync();
}
