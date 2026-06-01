using System.Security.Claims;
using System.Text.Json;

namespace Ecommerce.Admin.Web.Authentication;

internal static class JwtClaims
{
    public static IReadOnlyList<Claim>? Parse(string token)
    {
        var segments = token.Split('.');
        if (segments.Length != 3)
        {
            return null;
        }

        Dictionary<string, JsonElement>? payload;
        try
        {
            var json = Base64UrlDecode(segments[1]);
            payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }

        if (payload is null)
        {
            return null;
        }

        var claims = new List<Claim>();
        foreach (var (type, value) in payload)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                claims.AddRange(value.EnumerateArray()
                    .Select(item => new Claim(type, item.ToString())));
            }
            else
            {
                claims.Add(new Claim(type, value.ToString()));
            }
        }

        return claims;
    }

    public static bool IsExpired(IReadOnlyList<Claim> claims)
    {
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (exp is null || !long.TryParse(exp, out var seconds))
        {
            return true;
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }
}
