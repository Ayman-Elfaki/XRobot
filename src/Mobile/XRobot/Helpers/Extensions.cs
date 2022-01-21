using SkiaSharp;

namespace XRobot.Helpers;
public static class Extensions
{
    public static SKPoint Map(this SKPoint point, SKSize inSizeMin, SKSize inSizeMax, SKSize outSizeMin, SKSize outSizeMax)
    {

        var x = (point.X - inSizeMin.Width) * (outSizeMax.Width - outSizeMin.Width) / (inSizeMax.Width - inSizeMin.Width) + outSizeMin.Width;
        var y = (point.Y - inSizeMin.Height) * (outSizeMax.Height - outSizeMin.Height) / (inSizeMax.Height - inSizeMin.Height) + outSizeMin.Height;
        return new SKPoint(x, -y);
    }

    public static SKPoint Clamp(this SKPoint value, float min, float max)
    {
        return new SKPoint(Math.Clamp(value.X, min, max), Math.Clamp(value.Y, min, max));
    }

    public static T Scale<T>(this T value, T scale) where T : INumber<T>
    {
        return value * scale;
    }

    public static T Map<T>(this T value, T inValueMin, T inValueMax, T outValueMin, T outValueMax) where T : INumber<T>
    {
        return (value - inValueMin) * (outValueMax - outValueMin) / (inValueMax - inValueMin) + outValueMin;
    }

}
