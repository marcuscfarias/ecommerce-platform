using Ecommerce.Shared.Domain.Exceptions;

namespace Ecommerce.Shared.UnitTests.Domain.Exceptions;

public class BusinessRuleValidationExceptionTests
{
    //MethodName_StateUnderTest_ExpectedBehavior
    [Fact]
    public void Construct_StatusCode_ShouldReturn409()
    {
        //arrange
        var ex = new BusinessRuleValidationException("any message");
        
        //act
        
        //assert
        ex.StatusCode.ShouldBe(409);
    }
}