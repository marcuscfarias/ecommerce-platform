namespace Ecommerce.Shared.Domain.BusinessRules;

public interface IBusinessRule
{
    bool IsMet();
    string Error { get; }
}
