using System.Net;
using Ecommerce.Common.API.ErrorHandling;

namespace Ecommerce.Common.UnitTests.API.ErrorHandling;

public class ExceptionStatusCodeMapperTests
{
    [Fact]
    public void GetStatusCode_ArgumentNullException_ReturnsBadRequest()
    {
        var exception = new ArgumentNullException("param");

        var statusCode = ExceptionStatusCodeMapper.GetStatusCode(exception);

        statusCode.ShouldBe((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public void GetStatusCode_ArgumentOutOfRangeException_ReturnsBadRequest()
    {
        var exception = new ArgumentOutOfRangeException("param");

        var statusCode = ExceptionStatusCodeMapper.GetStatusCode(exception);

        statusCode.ShouldBe((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public void GetStatusCode_ArgumentException_ReturnsBadRequest()
    {
        var exception = new ArgumentException("message");

        var statusCode = ExceptionStatusCodeMapper.GetStatusCode(exception);

        statusCode.ShouldBe((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public void GetStatusCode_UnknownException_ReturnsInternalServerError()
    {
        var exception = new InvalidOperationException("message");

        var statusCode = ExceptionStatusCodeMapper.GetStatusCode(exception);

        statusCode.ShouldBe((int)HttpStatusCode.InternalServerError);
    }
}
