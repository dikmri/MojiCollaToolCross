using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MojiCollaTool;

public static class FontUtil
{
    private static List<string>? _fontNames;

    public static IReadOnlyList<string> GetFontNames()
    {
        if (_fontNames != null) return _fontNames;
        try
        {
            _fontNames = FontManager.Current.SystemFonts
                .Select(f => f.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }
        catch
        {
            _fontNames = new List<string> { "Arial", "MS Gothic" };
        }
        return _fontNames;
    }

    public static Dictionary<string, string> GetFontFamilies()
    {
        return GetFontNames().ToDictionary(n => n, n => n);
    }
}
