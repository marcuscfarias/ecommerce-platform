using System.Globalization;
using System.Security.Claims;
using Ecommerce.Kernel.Application.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Ecommerce.Kernel.API.Security;

internal sealed class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    public int UserId
    {
        get
        {
            var sub = accessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new InvalidOperationException("No authenticated user in the current context.");

            return int.Parse(sub, CultureInfo.InvariantCulture);
        }
    }
}
