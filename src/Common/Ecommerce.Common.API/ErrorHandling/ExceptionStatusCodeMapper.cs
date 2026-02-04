using System.Net;

namespace Ecommerce.Common.API.ErrorHandling;

public static class ExceptionStatusCodeMapper
{
    public static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}
