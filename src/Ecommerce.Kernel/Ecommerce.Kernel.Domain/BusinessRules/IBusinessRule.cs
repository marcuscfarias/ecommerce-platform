namespace Ecommerce.Kernel.Domain.BusinessRules;

public interface IBusinessRule
{
    bool IsMet();
    string ErrorMessage { get; }
}
