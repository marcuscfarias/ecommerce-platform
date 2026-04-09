namespace Ecommerce.Shared.Domain.BusinessRules;

public interface IBusinessRule
{
    bool IsMet();
    Exception CreateException();
}
