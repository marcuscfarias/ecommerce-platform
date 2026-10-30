namespace Ecommerce.Admin.Web.Services;

// Raised when a request outlives the threshold and lowered once the API answers, so a slow
// but healthy request doesn't leave the user looking at a stale warning. A request that never
// gets an answer keeps it up, so a final timeout still reads as a cold start, not a defect.
public sealed class ColdStartNotice
{
    public static readonly TimeSpan Threshold = TimeSpan.FromSeconds(10);

    public event Action? Changed;

    public bool IsVisible { get; private set; }

    public void Signal() => SetVisible(true);

    public void Clear() => SetVisible(false);

    private void SetVisible(bool visible)
    {
        if (IsVisible == visible)
        {
            return;
        }

        IsVisible = visible;
        Changed?.Invoke();
    }
}
