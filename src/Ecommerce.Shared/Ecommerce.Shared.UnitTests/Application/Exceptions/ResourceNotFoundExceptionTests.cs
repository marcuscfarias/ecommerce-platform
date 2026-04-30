using Ecommerce.Shared.Application.Exceptions;

namespace Ecommerce.Shared.UnitTests.Application.Exceptions;

public class ResourceNotFoundExceptionFormatTests
{
    [Fact]
    public void Construct_StatusCode_ShouldReturn404()
    {
        //arrange 
        var ex = new ResourceNotFoundExceptionFormat("Entity", 1);
        
        //act
        
        //assert
        ex.StatusCode.ShouldBe(404);
    }

    [Fact]
    public void Construct_MessageError_ShouldConcatenateMessage()
    {
        //arrange
        string entity = "Entity";
        int id = 1;
        var ex = new ResourceNotFoundExceptionFormat(entity, id);
        
        //act
        
        //assert
        ex.Message.ShouldBe($"{entity} with Id {id} couldn't be found.");
    }
}