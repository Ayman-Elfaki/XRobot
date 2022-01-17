
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace XRobot.Helpers;
public static class Brushes
{
    public static Color BrushesDark => Colors.SlateGray;
    public static SKColor[] ToBaseGradientColors(this Color color, bool isEnabled = true)
    {
        var eColors = new SKColor[]
        {
            color.WithLuminosity(0.66f).ToSKColor(),
            color.WithLuminosity(0.36f).ToSKColor(),
            color.WithLuminosity(0.35f).ToSKColor(),
        };

        var dColors = new SKColor[]
        {
           BrushesDark.WithLuminosity(0.66f).ToSKColor(),
           BrushesDark.WithLuminosity(0.36f).ToSKColor(),
           BrushesDark.WithLuminosity(0.35f).ToSKColor(),
        };

        return isEnabled ? eColors : dColors;
    }

    public static SKColor[] ToSocketGradientColor(this Color color, bool isEnabled = true)
    {
        var eColors = new SKColor[]
        {
            color.WithLuminosity(0.70f).ToSKColor(),
            color.WithLuminosity(0.65f).ToSKColor(),
            color.WithLuminosity(0.45f).ToSKColor()
        };

        var dColors = new SKColor[]
        {
           BrushesDark.WithLuminosity(0.70f).ToSKColor(),
           BrushesDark.WithLuminosity(0.65f).ToSKColor(),
           BrushesDark.WithLuminosity(0.45f).ToSKColor()
        };

        return isEnabled ? eColors : dColors;
    }

    public static SKColor[] BumpsGradientColor(this Color color, bool isEnabled = true)
    {
        var eColors = new SKColor[]
        {
            color.WithLuminosity(0.8f).ToSKColor(),
            color.WithLuminosity(0.4f).ToSKColor(),
            color.WithLuminosity(0.4f).ToSKColor()
        };

        var dColors = new SKColor[]
        {
           BrushesDark.WithLuminosity(0.8f).ToSKColor(),
           BrushesDark.WithLuminosity(0.4f).ToSKColor(),
           BrushesDark.WithLuminosity(0.4f).ToSKColor()
        };

        return isEnabled ? eColors : dColors;
    }

    public static SKColor[] ThumbGradientColor(this Color color, bool isEnabled = true)
    {
        var eColors = new SKColor[]
        {
            color.WithLuminosity(0.6f).ToSKColor(),
            color.WithLuminosity(0.3f).ToSKColor(),
        };

        var dColors = new SKColor[]
        {
           BrushesDark.WithLuminosity(0.6f).ToSKColor(),
           BrushesDark.WithLuminosity(0.3f).ToSKColor(),
        };

        return isEnabled ? eColors : dColors;
    }

    public static SKColor ToEngravedGradientColor(this Color color, bool isEnabled = true)
    {
        return isEnabled ? color.WithLuminosity(0.3f).ToSKColor() : BrushesDark.WithLuminosity(0.3f).ToSKColor();
    }


}
