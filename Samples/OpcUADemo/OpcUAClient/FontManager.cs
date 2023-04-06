using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;

namespace OpcUAClient;

public class FontManager : IFontManagerImpl
{
    private readonly Typeface[] _customTypefaces;
    private readonly string _defaultFamilyName;

    private readonly Typeface _defaultTypeface = new("avares://CameraDemo/Assets/msyh.ttc#微软雅黑");

    private readonly string[] _bcp47 =
        { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };

    public FontManager()
    {
        _customTypefaces = new[] { _defaultTypeface };
        _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
    }

    public string GetDefaultFontFamilyName() => _defaultFamilyName;

    public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false) =>
        _customTypefaces.Select(x => x.FontFamily.Name);

    public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
        CultureInfo culture, out Typeface typeface)
    {
        foreach (var customTypeface in _customTypefaces)
        {
            if (customTypeface.GlyphTypeface.GetGlyph((uint)codepoint) == 0)
            {
                continue;
            }

            typeface = new Typeface(customTypeface.FontFamily.Name, fontStyle, fontWeight);

            return true;
        }

        var fallback = SKFontManager.Default.MatchCharacter(fontFamily?.Name, (SKFontStyleWeight)fontWeight,
            SKFontStyleWidth.Normal, (SKFontStyleSlant)fontStyle, _bcp47, codepoint);

        typeface = new Typeface(fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

        return true;
    }

    public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface)
    {
        var skTypeface = typeface.FontFamily.Name switch
        {
            FontFamily.DefaultFontFamilyName => SKTypeface.FromFamilyName(_defaultTypeface.FontFamily.Name),
            "微软雅黑" => SKTypeface.FromFamilyName(_defaultTypeface.FontFamily.Name),
            _ => SKTypeface.FromFamilyName(_defaultTypeface.FontFamily.Name)
        };

        return new GlyphTypefaceImpl(skTypeface);
    }
}