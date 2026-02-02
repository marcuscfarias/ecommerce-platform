namespace Ecommerce.Common.Domain.BusinessRules;

public interface IBusinessRule
{
    bool IsMet();
    string Error { get; }
}