namespace Ecommerce.Admin.Web.Authentication;

public sealed class AccessDeniedNotice
{
    public event Action? Raised;

    public void Raise() => Raised?.Invoke();
}
