namespace Ecommerce.Shop.Web.Services;

// A one-way latch. Once the visitor has been told to wait and reload, taking the notice
// back down would contradict the instruction — and on the path where the request finally
// times out, the generic error would go back to looking like a defect.
public sealed class ColdStartNotice
{
    public static readonly TimeSpan Threshold = TimeSpan.FromSeconds(3);

    public event Action? Changed;

    public bool IsVisible { get; private set; }

    public void Signal()
    {
        if (IsVisible)
        {
            return;
        }

        IsVisible = true;
        Changed?.Invoke();
    }
}
