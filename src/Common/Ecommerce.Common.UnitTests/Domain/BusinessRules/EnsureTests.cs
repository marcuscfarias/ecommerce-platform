using Ecommerce.Common.Domain.BusinessRules;

namespace Ecommerce.Common.UnitTests.Domain.BusinessRules;

public class EnsureTests
{
    [Fact]
    public void Validate_RuleIsMet_DoesNotThrow()
    {
        //arrange
        var businessRule = new FakeRule(11);

        //act & assert
        Should.NotThrow(() => Ensure.That(businessRule));
    }

    [Fact]
    public void Validate_RuleIsNotMet_ThrowsExceptionWithErrorMessage()
    {
        //arrange
        var businessRule = new FakeRule(10);

        //act
        var exception = Should.Throw<Exception>(
            () => Ensure.That(businessRule));

        //assert
        exception.Message.ShouldBe("Fake business rule was not met");
    }
}
