using Avalonia.Media;
using Mekatrol.CAM.Core.Converter;
using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Render;
using System.Text.RegularExpressions;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public class CssParser
{
    public const float DefaultFontSize = 4.233f; // CSS default 16px ≈ 4.233 mm

    private const string ClassNamePattern = @"\.-?([_a-zA-Z]+[_a-zA-Z0-9-]*)\s*\{(\s*.*\s*)}";
    private const string FontSizePattern = @"([0-9]*\.{0,1}[0-9]+)([a-zA-Z%]*)(\/([0-9]*\.{0,1}[0-9]+)([a-zA-Z%]*)){0,1}";

    private const string Font = "font:";
    private const string FontFamily = "font-family:";
    private const string FontSize = "font-size:";
    private const string FontWeightAttr = "font-weight:";

    public static IDictionary<string, CssClass> Parse(string css)
    {
        var classes = new Dictionary<string, CssClass>(StringComparer.OrdinalIgnoreCase);

        var matches = Regex.Matches(css, ClassNamePattern)
            .Cast<Match>()
            .Where(m => m.Success)
            .ToList();

        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];

            var className = match.Groups[1].Value;
            var classContent = match.Groups[2].Value;

            var lines = classContent
                .Split(['\r', '\n', ';'], StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            FontDescription? font = null;

            foreach (var line in lines)
            {
                if (line.StartsWith(Font))
                {
                    var extracted = ExtractFont(line);
                    if (extracted != null)
                    {
                        font = extracted;
                    }
                }

                if (line.StartsWith(FontFamily))
                {
                    font ??= new(RenderExtensions.DefaultFontFamilyName, 30, FontStyle.Normal, FontWeight.Normal);

                    ExtractFontFamily(line, font);
                }

                if (line.StartsWith(FontSize))
                {
                    font ??= new(RenderExtensions.DefaultFontFamilyName, 30, FontStyle.Normal, FontWeight.Normal);

                    ExtractFontSize(line, font);
                }

                if (line.StartsWith(FontWeightAttr))
                {
                    font ??= new(RenderExtensions.DefaultFontFamilyName, 30, FontStyle.Normal, FontWeight.Normal);

                    ExtractFontWeight(line, font);
                }
            }

            classes[className] = new CssClass(className)
            {
                Font = font
            };
        }

        return classes;
    }

    public static void ExtractFontFamily(string line, FontDescription font)
    {
        var family = line.Remove(line.IndexOf(FontFamily), FontFamily.Length).Trim();
        font.FamilyName = family;
    }

    public static void ExtractFontSize(string input, FontDescription font)
    {
        if (string.IsNullOrWhiteSpace(input)) { return; }

        // Accept either "font-size: 14px" or "14px" or "120%"
        var s = input;
        var i = s.IndexOf("font-size:", StringComparison.OrdinalIgnoreCase);

        if (i >= 0)
        {
            s = s[(i + "font-size:".Length)..];
        }

        s = s.Trim();

        // number + unit [+ optional /line-height which we ignore]
        const string FontSizePattern = @"^\s*([0-9]*\.?[0-9]+)\s*([a-zA-Z%]*)\s*(?:/\s*[0-9]*\.?[0-9]+[a-zA-Z%]*)?\s*$";
        var m = Regex.Match(s, FontSizePattern);
        if (!m.Success) { return; }

        var val = float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        var unit = m.Groups[2].Value;
        if (string.IsNullOrWhiteSpace(unit)) { unit = "px"; }

        // Resolve to mm. Relative units use current font size as the context.
        font.Size = UnitSizeConverter.ConvertGraphicSizeToMM(val, unit, currentFontSizeMm: (float)font.Size);
    }

    public static void ExtractFontWeight(string line, FontDescription font)
    {
        line = line.Remove(line.IndexOf(FontWeightAttr), FontWeightAttr.Length).Trim();
        if (line.Trim().Contains("italic"))
        {
            font.Style |= FontStyle.Italic;
        }

        if (line.Trim().Contains("bold"))
        {
            font.Weight |= FontWeight.Bold;
        }
    }

    public static FontDescription? ExtractFont(string fontDescription)
    {
        if (string.IsNullOrWhiteSpace(fontDescription))
        {
            return null;
        }

        var token = NextToken(ref fontDescription);

        // Make sure this is a font definition
        if (token != "font:")
        {
            return null;
        }

        // Ref: https://developer.mozilla.org/en-US/docs/Web/CSS/font
        /**************************************************************************************************
         * The font property may be specified as either a single keyword, which will select a system font, 
         * or as a shorthand for various font-related properties.
         * If font is specified as a system keyword, it must be one of: 
         *      caption, icon, menu, message-box, small-caption, status-bar.
         * 
         * If font is specified as a shorthand for several font-related properties, then:
         * 1. it must include values for:
         *      font-size
         *      font-family
         *
         * 2. it may optionally include values for:
         *      font-style
         *      font-variant
         *      font-weight
         *      font-stretch
         *      line-height
         * 
         * Rules:
         *   1. font-style, font-variant and font-weight must precede font-size
         *   2. font-variant may only specify the values defined in CSS 2.1, that is normal and small-caps
         *   3. font-stretch may only be a single keyword value.
         *   4. line-height must immediately follow font-size, preceded by "/", like this: "16px/3"
         *   5. font-family must be the last value specified.
         **********************************************************************************/

        // Set our defaults
        var fontStyle = FontStyle.Normal;
        var fontWeight = FontWeight.Normal;
        var fontSize = DefaultFontSize;

        var tokens = new List<string>();
        while ((token = NextToken(ref fontDescription)) != string.Empty)
        {
            tokens.Add(token);
        }

        // If tokens count < 3 (third being ';') then we ignore as it is not well formed
        // or it is just a single reference to caption, icon, menu, message-box, small-caption, status-bar
        // which we do not support
        if (tokens.Count < 3 || tokens[^1] != ";")
        {
            return null;
        }

        for (var i = 0; i < tokens.Count - 2 /* Ignore ';' end token, and font family names token  */; i++)
        {
            token = tokens[i];

            if (token == "bold")
            {
                fontWeight = FontWeight.Bold;
            }

            else if (token == "italic")
            {
                fontStyle |= FontStyle.Italic;
            }

            var match = Regex.Match(token, FontSizePattern);
            if (match.Success)
            {
                fontSize = float.Parse(match.Groups[1].Value);
                var sizeUnit = match.Groups[2].Value;
                fontSize = UnitSizeConverter.ConvertGraphicSizeToMM(fontSize, sizeUnit);
                break;
            }
        }

        // Theoretically the font family token should be the second last token (last being ';' token)
        var fontFamily = RenderExtensions.BestFontFamily(tokens[^2]);

        return new FontDescription(fontFamily.Name, fontSize, fontStyle, fontWeight);
    }
    private static string NextToken(ref string value)
    {
        // Clear any whitespace
        value = value.Trim();

        // Token created
        var token = string.Empty;

        var endQuote = '\0'; // '\0' means not currently in a quoted value

        while (value.Length > 0)
        {
            // Get the next character
            var ch = value[0];

            // End of value?
            if (ch == ';' && !string.IsNullOrWhiteSpace(token))
            {
                // Return the token that was in progress
                return token;
            }

            // Remove the character
            value = value[1..];

            // End of value?
            if (ch == ';')
            {
                // End of value
                return ";";
            }

            // If we are not in a quote and a space is found then return the token
            if (endQuote == '\0' && char.IsWhiteSpace(ch))
            {
                value = value.Trim();
                return token;
            }

            // Add this char to the token
            token += ch;

            // A, open quote?
            if ((ch == '"' || ch == '\'') && endQuote == '\0')
            {
                endQuote = ch;
                continue;
            }

            // End of quote?
            if (ch == endQuote)
            {
                value = value.Trim();
                endQuote = '\0';

                if (value.Length > 0 && value[0] == ',')
                {
                    token += ',';

                    // Remove the comma character and trim
                    value = value[1..].Trim();

                    continue;
                }
                return token;
            }
        }

        return string.Empty;
    }
}
