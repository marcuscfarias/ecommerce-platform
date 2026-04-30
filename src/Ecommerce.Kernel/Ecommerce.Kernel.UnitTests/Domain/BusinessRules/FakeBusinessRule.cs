using Ecommerce.Kernel.Domain.BusinessRules;

namespace Ecommerce.Kernel.UnitTests.Domain.BusinessRules;

internal sealed class FakeBusinessRule(int someNumber) : IBusinessRule
{
    public bool IsMet() => someNumber > 10;
    public string ErrorMessage => "Fake business rule was not met";
}
