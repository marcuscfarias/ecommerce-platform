using Ecommerce.Common.Domain.BusinessRules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules;

internal sealed class FakeRule : IRule
{
    private readonly int _someNumber;

    internal FakeRule(int someNumber) =>
        _someNumber = someNumber;

    public bool IsMet() => _someNumber > 10;

    public Exception CreateException() => new Exception("Fake business rule was not met");
}