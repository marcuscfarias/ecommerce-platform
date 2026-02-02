using Ecommerce.Common.Domain.BusinessRules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules;

internal sealed class FakeBusinessRule : IBusinessRule
{
    private readonly int _someNumber;

    internal FakeBusinessRule(int someNumber) =>
        _someNumber = someNumber;

    public bool IsMet() => _someNumber > 10;

    public string Error => "Fake business businessRule was not met";
    public Exception CreateException(string errorMessage) => new Exception(errorMessage);
}