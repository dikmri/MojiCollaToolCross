using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace WpfColorPicker;

public class HueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double hue = ((double)value! / 100) * 360;
        byte r, g, b;

        if (hue <= 60) { r = 255; g = (byte)(hue / 60 * 255); b = 0; }
        else if (hue <= 120) { r = (byte)((120 - hue) / 60 * 255); g = 255; b = 0; }
        else if (hue <= 180) { r = 0; g = 255; b = (byte)((hue - 120) / 60 * 255); }
        else if (hue <= 240) { r = 0; g = (byte)((240 - hue) / 60 * 255); b = 255; }
        else if (hue <= 300) { r = (byte)((hue - 240) / 60 * 255); g = 0; b = 255; }
        else { r = 255; g = 0; b = (byte)((360 - hue) / 60 * 255); }

        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
