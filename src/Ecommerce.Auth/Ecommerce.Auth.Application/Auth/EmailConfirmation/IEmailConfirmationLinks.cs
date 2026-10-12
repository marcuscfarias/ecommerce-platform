namespace Ecommerce.Auth.Application.Auth.EmailConfirmation;

public interface IEmailConfirmationLinks
{
    Uri ShopHome { get; }

    Uri ConfirmationLink(string token);

    Uri AfterConfirmation(bool succeeded);
}
