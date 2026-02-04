namespace Ecommerce.Common.Domain.BusinessRules;

public interface IRule
{
    bool IsMet();
    Exception CreateException();
}