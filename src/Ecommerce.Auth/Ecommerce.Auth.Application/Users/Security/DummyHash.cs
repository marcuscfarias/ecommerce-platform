namespace Ecommerce.Auth.Application.Users.Security;

// BCrypt hash of a random string at cost factor 12. Used only to equalise timing
// when the email does not exist — prevents user enumeration via response latency.
// Not a secret; never compared against a real password.
internal static class DummyHash
{
    public const string Value = "$2a$12$cFImGngfLrmQcxOsR1Np.Okd210KBzNKi/mxJU9NVmuaw8iKjf4Ve";
}
