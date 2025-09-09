using Avalonia.Media;
using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public class TextParser : SvgParserBase
{
    private readonly FontDescription _currentFont;

    public TextParser(ILogger logger) : base(logger)
    {
        var fontFamily = RenderExtensions.BestFontFamily(RenderExtensions.DefaultFontFamilyName);
        _currentFont = new FontDescription(fontFamily.Name, 30, FontStyle.Normal, FontWeight.Normal);
    }

    public IGeometricEntity ParseTextElement(XElement element)
    {
        AssertIsTag(element, "text");

        var x = GetAttributeDoubleValue(element, "x").Value ?? 0.0;
        var y = GetAttributeDoubleValue(element, "y").Value ?? 0.0;

        // Extract text elements as entity path
        var textPath = (PathEntity)ParseTextSubElement(element, "text", _currentFont, x, y);

        return new PathEntity(textPath.Location.X, textPath.Location.Y, textPath.Entities, /*closed:*/ false, ParseTransformAttribute(element));
    }

    private IGeometricEntity ParseTextSubElement(
        XElement element,
        string tag,
        FontDescription? parentFont,
        double parentX,
        double parentY)
    {
        AssertIsTag(element, tag);

        var xAttr = GetAttributeDoubleValue(element, "x").Value;
        var yAttr = GetAttributeDoubleValue(element, "y").Value;
        var x = xAttr ?? parentX;
        var yUser = yAttr ?? parentY;

        var transform = ParseTransformAttribute(element);

        var font = parentFont ?? _currentFont;

        var className = GetAttributeValue(element, "class");
        if (className != null && _cssClasses.TryGetValue(className, out var cssFontClass) && cssFontClass?.Font != null)
        {
            font = cssFontClass.Font;
        }

        var style = GetAttributeValue(element, "style");
        var parsedFont = ParseFontFromStyle(style);
        if (parsedFont != null)
        {
            font = parsedFont;
        }

        var fontSizeValue = GetAttributeValue(element, "font-size");
        if (fontSizeValue != null)
        {
            CssParser.ExtractFontSize(fontSizeValue, font);
        }

        var align = TextAlignment.Left;
        var textAnchor = GetAttributeValue(element, "text-anchor");
        if (textAnchor != null)
        {
            align = textAnchor.ToLower() switch
            {
                "middle" => TextAlignment.Center,
                "end" => TextAlignment.Right,
                _ => TextAlignment.Left,
            };
        }

        var domBaseline = (GetAttributeValue(element, "dominant-baseline") ?? "").ToLower();

        using var tf = SKTypeface.FromFamilyName(
            font.FamilyName,
            SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright);

        using var paint = new SKPaint { Typeface = tf, TextSize = (float)font.Size, IsAntialias = true };
        paint.GetFontMetrics(out var fm);

        var baselineY = AdjustBaselineYPx(yUser, domBaseline, (float)font.Size, fm);

        var runs = new List<IGeometricEntity>();
        var cursorX = x;
        var cursorBaselineY = baselineY;

        foreach (var node in element.Nodes())
        {
            if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA)
            {
                var s = ((XText)node).Value;
                if (string.IsNullOrWhiteSpace(s)) { continue; }

                var t = s.Trim();
                var textEntity = new TextEntity(cursorX, cursorBaselineY, t, font, align, new GeometryTransform());
                runs.Add(textEntity);

                var w = paint.MeasureText(t);
                var gap = 0.25f * (float)font.Size;
                cursorX += w + gap;
                continue;
            }

            if (node.NodeType == XmlNodeType.Element)
            {
                var child = (XElement)node;
                if (child.Name.LocalName != "tspan") { continue; }

                var tspanFont = new FontDescription(font.FamilyName, font.Size, font.Style, font.Weight);

                var tspanFontSize = GetAttributeValue(child, "font-size");
                if (tspanFontSize != null)
                {
                    CssParser.ExtractFontSize(tspanFontSize, tspanFont);
                }

                var tspanX = GetAttributeDoubleValue(child, "x").Value;
                var tspanY = GetAttributeDoubleValue(child, "y").Value;
                var dxStr = GetAttributeValue(child, "dx");
                var dyStr = GetAttributeValue(child, "dy");

                if (tspanX.HasValue) { cursorX = tspanX.Value; }
                if (tspanY.HasValue)
                {
                    using var tspanPaint = new SKPaint { Typeface = tf, TextSize = (float)tspanFont.Size, IsAntialias = true };
                    tspanPaint.GetFontMetrics(out var tfm);
                    var tspanDom = (GetAttributeValue(child, "dominant-baseline") ?? domBaseline);
                    cursorBaselineY = AdjustBaselineYPx(tspanY.Value, tspanDom, (float)tspanFont.Size, tfm);
                }

                if (!string.IsNullOrWhiteSpace(dxStr))
                {
                    cursorX += ParseLengthPx(dxStr!, (float)tspanFont.Size);
                }
                if (!string.IsNullOrWhiteSpace(dyStr))
                {
                    cursorBaselineY += ParseLengthPx(dyStr!, (float)tspanFont.Size);
                }

                var tspanAnchor = GetAttributeValue(child, "text-anchor");
                var tspanAlign = align;
                if (tspanAnchor != null)
                {
                    tspanAlign = tspanAnchor.ToLower() switch
                    {
                        "middle" => TextAlignment.Center,
                        "end" => TextAlignment.Right,
                        _ => TextAlignment.Left,
                    };
                }

                var t = child.Value;
                if (string.IsNullOrWhiteSpace(t)) { continue; }
                t = t.Trim();

                var textEntity = new TextEntity(cursorX, cursorBaselineY, t, tspanFont, tspanAlign, new GeometryTransform());
                runs.Add(textEntity);

                using var tspanPaint2 = new SKPaint { Typeface = tf, TextSize = (float)tspanFont.Size, IsAntialias = true };
                var w2 = tspanPaint2.MeasureText(t);
                var gap2 = 0.25f * (float)tspanFont.Size;
                cursorX += w2 + gap2;
            }
        }

        return new PathEntity(x, yUser, runs, false, transform);
    }

    private static double AdjustBaselineYPx(double yAnchor, string dominantBaseline, float fontPx, SKFontMetrics fm)
    {
        return dominantBaseline switch
        {
            "middle" or "central" => yAnchor + (fm.Ascent + fm.Descent) * 0.5f,
            "hanging" => yAnchor - 0.2f * fontPx,
            _ => yAnchor,
        };
    }

    private static double ParseLengthPx(string v, float currentFontPx)
    {
        v = v.Trim().ToLowerInvariant();

        if (v.EndsWith("em"))
        {
            var n = double.Parse(v[..^2], CultureInfo.InvariantCulture);
            return n * currentFontPx;
        }
        if (v.EndsWith("px"))
        {
            var n = double.Parse(v[..^2], CultureInfo.InvariantCulture);
            return n;
        }
        if (v.EndsWith('%'))
        {
            var n = double.Parse(v[..^1], CultureInfo.InvariantCulture);
            return (n / 100.0) * currentFontPx;
        }

        // raw number: treat as px per SVG
        return double.Parse(v, CultureInfo.InvariantCulture);
    }

    private static FontDescription? ParseFontFromStyle(string? style)
    {
        if (string.IsNullOrWhiteSpace(style))
        {
            return null;
        }

        var css = CssParser.Parse($".font {{ {style} }}");

        if (!css.TryGetValue("font", out var value))
        {
            return null;
        }

        return value.Font;
    }

}
