using Ecommerce.Shared.Domain.BusinessRules;

namespace Ecommerce.Shared.UnitTests.Domain.BusinessRules;

internal sealed class FakeBusinessRule(int someNumber) : IBusinessRule
{
    public bool IsMet() => someNumber > 10;

    public Exception CreateException() =>
        new InvalidOperationException("Fake business rule was not met");
}
