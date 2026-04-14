namespace Ecommerce.Shared.Domain.BusinessRules;

public interface IBusinessRule
{
    bool IsMet();
    string ErrorMessage { get; }
}
