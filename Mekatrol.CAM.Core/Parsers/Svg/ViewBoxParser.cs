using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public enum OutputUnit { Millimeter, Inch }

public readonly record struct SvgViewport(double MinX, double MinY, double Width, double Height);
public readonly record struct SvgSize(double Width, double Height, OutputUnit Unit);

public readonly record struct SvgViewInfo(SvgViewport ViewBox, SvgSize PhysicalSize);

public static class ViewBoxParser
{
    private static readonly Regex ViewBoxRegex = new(
        @"^\s*(?<minx>-?\d+(?:\.\d+)?)\s+(?<miny>-?\d+(?:\.\d+)?)\s+(?<w>\d+(?:\.\d+)?)\s+(?<h>\d+(?:\.\d+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static SvgViewInfo ReadViewBox(XElement element, OutputUnit outUnit)
    {
        SvgParserBase.AssertIsTag(element, "svg");

        // 1. Width/Height physical size
        var w = ParseLengthToMm((string?)element.Attribute("width")) ?? 300.0 * 25.4 / 96.0;
        var h = ParseLengthToMm((string?)element.Attribute("height")) ?? 150.0 * 25.4 / 96.0;

        if (outUnit == OutputUnit.Inch)
        {
            w /= 25.4;
            h /= 25.4;
        }

        var physicalSize = new SvgSize(w, h, outUnit);

        // 2. ViewBox
        var vbAttr = (string?)element.Attribute("viewBox");
        SvgViewport viewBox;
        if (!string.IsNullOrWhiteSpace(vbAttr))
        {
            var m = ViewBoxRegex.Match(vbAttr);
            if (m.Success)
            {
                var minx = double.Parse(m.Groups["minx"].Value, CultureInfo.InvariantCulture);
                var miny = double.Parse(m.Groups["miny"].Value, CultureInfo.InvariantCulture);
                var vw = double.Parse(m.Groups["w"].Value, CultureInfo.InvariantCulture);
                var vh = double.Parse(m.Groups["h"].Value, CultureInfo.InvariantCulture);
                viewBox = new SvgViewport(minx, miny, vw, vh);
            }
            else
            {
                // malformed → derive from physical size
                viewBox = new SvgViewport(0, 0, w, h);
            }
        }
        else
        {
            viewBox = new SvgViewport(0, 0, w, h);
        }

        return new SvgViewInfo(viewBox, physicalSize);
    }

    // --- Helpers ---
    // Returns mm, null if malformed
    private static double? ParseLengthToMm(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return null;
        }

        s = s.Trim();

        if (s.EndsWith("%"))
        {
            return null; // % not useful here
        }

        var i = 0;
        while (i < s.Length && (char.IsDigit(s[i]) || s[i] is '+' or '-' || s[i] == '.' || s[i] == 'e' || s[i] == 'E'))
        {
            i++;
        }

        var nstr = s[..i];
        var ustr = s[i..].Trim().ToLowerInvariant();

        if (!double.TryParse(nstr, NumberStyles.Float, CultureInfo.InvariantCulture, out var n))
        {
            return null;
        }

        return ustr switch
        {
            "" => n * 25.4 / 96.0,       // unitless → px → mm
            "px" => n * 25.4 / 96.0,
            "mm" => n,
            "cm" => n * 10.0,
            "in" => n * 25.4,
            "pt" => (n / 72.0) * 25.4,
            "pc" => (n / 6.0) * 25.4,     // 1pc = 1/6 in
            _ => null
        };
    }
}
