using MudBlazor;

namespace Ecommerce.Admin.Web.Theme;

public static class AmazonTheme
{
    public const string SquidInk = "#131A22";
    public const string Slate = "#232F3E";
    public const string Orange = "#FF9900";
    public const string GoldCtaStart = "#FFD814";
    public const string GoldCtaEnd = "#F7CA00";
    public const string LinkTeal = "#007185";
    public const string PriceRed = "#C40000";
    public const string Ink = "#0F1111";
    public const string Slate60 = "#565959";
    public const string BorderGray = "#D5D9D9";
    public const string Divider = "#E7E7E7";

    public static MudTheme Build() => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Orange,
            Secondary = Slate,
            Tertiary = LinkTeal,
            AppbarBackground = SquidInk,
            AppbarText = "#FFFFFF",
            DrawerBackground = Slate,
            DrawerText = "#DCE3EA",
            DrawerIcon = "#DCE3EA",
            Background = "#FFFFFF",
            Surface = "#FFFFFF",
            TextPrimary = Ink,
            TextSecondary = Slate60,
            ActionDefault = Slate60,
            Divider = Divider,
            LinesDefault = BorderGray,
            LinesInputs = "#888C8C",
            Error = PriceRed,
            Success = "#007600",
            Info = LinkTeal,
            Warning = "#F08804",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Hanken Grotesk", "Helvetica Neue", "Arial", "sans-serif"],
            },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
        },
    };
}
