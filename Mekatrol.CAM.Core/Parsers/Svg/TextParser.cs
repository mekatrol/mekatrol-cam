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
    // Default font used when no explicit font information is provided.
    private readonly FontDescription _defaultFont;

    public TextParser(ILogger logger) : base(logger)
    {
        // Resolve the best default font family available on the system.
        var fontFamily = RenderExtensions.BestFontFamily(RenderExtensions.DefaultFontFamilyName);
        _defaultFont = new FontDescription(fontFamily.Name, 30, FontStyle.Normal, FontWeight.Normal);
    }

    // Entry point for parsing an SVG <text> element into a geometric entity.
    public IGeometricEntity ParseTextElement(XElement element)
    {
        AssertIsTag(element, "text");

        // Get explicit x/y positions if provided.
        var x = GetAttributeDoubleValue(element, "x").Value ?? 0.0;
        var y = GetAttributeDoubleValue(element, "y").Value ?? 0.0;

        // Delegate to sub-element parser for handling runs and tspans.
        var path = (PathEntity)ParseTextSubElement(
            element: element,
            parentFont: _defaultFont,
            parentX: x,
            parentY: y);

        // Wrap text characters with a path entity.
        return new PathEntity(path.Location.X, path.Location.Y, path.Entities, false, ParseTransformAttribute(element));
    }

    // Core parser for a <text> or <tspan> element and its children.
    private IGeometricEntity ParseTextSubElement(
        XElement element,
        FontDescription? parentFont,
        double parentX,
        double parentY)
    {
        // Inherit or override element positioning.
        var x = GetAttributeDoubleValue(element, "x").Value ?? parentX;
        var yAnchor = GetAttributeDoubleValue(element, "y").Value ?? parentY;

        // Resolve transform chain (rotate, scale, translate, etc.).
        var transform = ParseTransformAttribute(element);

        // Determine font used by this element, possibly overridden by CSS or inline style.
        var font = ResolveFont(element, parentFont ?? _defaultFont);

        // Determine text alignment (start, middle, end).
        var align = ResolveAlignment(element);

        // Baseline positioning for vertical alignment.
        var dominantBaseline = (GetAttributeValue(element, "dominant-baseline") ?? string.Empty).ToLowerInvariant();

        // Create font resources (SkiaSharp font face and paint).
        using var tf = CreateTypeface(font);
        using var paint = CreatePaint(tf, (float)font.Size);
        paint.GetFontMetrics(out var fm);

        // Compute Y coordinate adjusted for baseline.
        var baselineY = ComputeBaselineYPx(yAnchor, dominantBaseline, (float)font.Size, fm);

        // Parse text runs (plain text and tspans).
        var runs = ParseTextRuns(element, tf, align, baselineY, font);

        // Return aggregated path entity.
        return new PathEntity(x, yAnchor, runs, false, transform);
    }

    // ---------- Font, alignment, and metrics helpers ----------

    // Resolve the font by checking CSS classes, inline style, and font-size attribute.
    private FontDescription ResolveFont(XElement element, FontDescription baseFont)
    {
        var font = new FontDescription(baseFont.FamilyName, baseFont.Size, baseFont.Style, baseFont.Weight);

        // Check for CSS class reference.
        var className = GetAttributeValue(element, "class");
        if (className != null &&
            _cssClasses.TryGetValue(className, out var cssClass) &&
            cssClass?.Font != null)
        {
            font = cssClass.Font;
        }

        // Parse inline style attribute.
        var style = GetAttributeValue(element, "style");
        var styled = ParseFontFromStyle(style);
        if (styled != null)
        {
            font = styled;
        }

        // Explicit font-size attribute overrides.
        var fontSize = GetAttributeValue(element, "font-size");
        if (fontSize != null)
        {
            CssParser.ExtractFontSize(fontSize, font);
        }

        return font;
    }

    // Resolve text alignment from SVG attribute "text-anchor".
    private static TextAlignment ResolveAlignment(XElement element)
    {
        var textAnchor = GetAttributeValue(element, "text-anchor");
        if (textAnchor == null)
        {
            return TextAlignment.Left;
        }

        return textAnchor.ToLowerInvariant() switch
        {
            "middle" => TextAlignment.Center,
            "end" => TextAlignment.Right,
            _ => TextAlignment.Left,
        };
    }

    // Create a Skia typeface from the font description.
    private static SKTypeface CreateTypeface(FontDescription font)
    {
        return SKTypeface.FromFamilyName(
            familyName: font.FamilyName,
            weight: SKFontStyleWeight.Normal,
            width: SKFontStyleWidth.Normal,
            slant: SKFontStyleSlant.Upright);
    }

    // Create a Skia paint object configured for text rendering.
    private static SKPaint CreatePaint(SKTypeface tf, float sizePx)
    {
        return new SKPaint
        {
            Typeface = tf,
            TextSize = sizePx,
            IsAntialias = true,
        };
    }

    // Compute Y position adjusted for dominant-baseline property.
    private static double ComputeBaselineYPx(double yAnchor, string dominantBaseline, float fontPx, SKFontMetrics fm)
    {
        return dominantBaseline switch
        {
            "middle" or "central" => yAnchor + (fm.Ascent + fm.Descent) * 0.5f,
            "hanging" => yAnchor - 0.2f * fontPx,
            _ => yAnchor,
        };
    }

    // ---------- Text run parsing ----------

    // Parse inline text runs and <tspan> children into a list of entities.
    private List<IGeometricEntity> ParseTextRuns(
        XElement element,
        SKTypeface tf,
        TextAlignment align,
        double baselineY,
        FontDescription font)
    {
        var runs = new List<IGeometricEntity>();

        // Initialize cursor positions.
        var cursorX = GetAttributeDoubleValue(element, "x").Value ?? 0.0;
        var cursorBaselineY = baselineY;

        // Walk through child nodes.
        foreach (var node in element.Nodes())
        {
            // Handle raw text content.
            if (node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.CDATA)
            {
                var s = ((XText)node).Value;
                if (string.IsNullOrWhiteSpace(s)) { continue; }

                var text = s.Trim();
                AddTextRun(runs, tf, text, ref cursorX, ref cursorBaselineY, align, font);
                continue;
            }

            // Handle <tspan> child nodes for rich text spans.
            if (node.NodeType == XmlNodeType.Element)
            {
                var child = (XElement)node;
                if (!child.Name.LocalName.Equals("tspan", StringComparison.OrdinalIgnoreCase)) { continue; }

                ParseTspanNode(child, tf, align, ref cursorX, ref cursorBaselineY, runs, font);
            }
        }

        return runs;
    }

    // Parse a <tspan> node and adjust cursor position accordingly.
    private void ParseTspanNode(
        XElement tspan,
        SKTypeface tf,
        TextAlignment parentAlign,
        ref double cursorX,
        ref double cursorBaselineY,
        List<IGeometricEntity> runs,
        FontDescription inheritedFont)
    {
        // Clone inherited font.
        var font = new FontDescription(inheritedFont.FamilyName, inheritedFont.Size, inheritedFont.Style, inheritedFont.Weight);

        // Override font-size if present.
        var tspanFontSize = GetAttributeValue(tspan, "font-size");
        if (tspanFontSize != null)
        {
            CssParser.ExtractFontSize(tspanFontSize, font);
        }

        // Handle explicit positioning overrides (x, y, dx, dy).
        var xAttr = GetAttributeDoubleValue(tspan, "x").Value;
        var yAttr = GetAttributeDoubleValue(tspan, "y").Value;
        var dxStr = GetAttributeValue(tspan, "dx");
        var dyStr = GetAttributeValue(tspan, "dy");

        if (xAttr.HasValue)
        {
            cursorX = xAttr.Value;
        }

        if (yAttr.HasValue)
        {
            using var tempPaint = CreatePaint(tf, (float)font.Size);
            tempPaint.GetFontMetrics(out var tfm);
            var dom = (GetAttributeValue(tspan, "dominant-baseline") ?? GetAttributeValue(tspan.Parent ?? tspan, "dominant-baseline") ?? string.Empty).ToLowerInvariant();
            cursorBaselineY = ComputeBaselineYPx(yAttr.Value, dom, (float)font.Size, tfm);
        }

        if (!string.IsNullOrWhiteSpace(dxStr))
        {
            cursorX += ParseLengthPx(dxStr!, (float)font.Size);
        }

        if (!string.IsNullOrWhiteSpace(dyStr))
        {
            cursorBaselineY += ParseLengthPx(dyStr!, (float)font.Size);
        }

        // Resolve alignment override if specified.
        var align = ResolveAlignment(tspan) == TextAlignment.Left ? parentAlign : ResolveAlignment(tspan);

        // Extract trimmed text content.
        var text = tspan.Value?.Trim();
        if (string.IsNullOrWhiteSpace(text)) { return; }

        // Add run entity for the tspan text.
        AddTextRun(runs, tf, text!, ref cursorX, ref cursorBaselineY, align, font);
    }

    // Add a text run entity and advance cursor position.
    private void AddTextRun(
        List<IGeometricEntity> runs,
        SKTypeface tf,
        string text,
        ref double cursorX,
        ref double cursorBaselineY,
        TextAlignment align,
        FontDescription font)
    {
        // Create entity for text run.
        var run = new TextEntity(cursorX, cursorBaselineY, text, font, align, new GeometryTransform());
        runs.Add(run);

        // Advance cursor horizontally by measured width plus gap.
        using var paint = CreatePaint(tf, (float)font.Size);
        var width = paint.MeasureText(text);
        var gap = 0.25f * (float)font.Size;
        cursorX += width + gap;
    }

    // ---------- Unit parsing ----------

    // Parse a length string (em, px, %, or raw number) into pixel units.
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

        // Default: interpret as raw px.
        return double.Parse(v, CultureInfo.InvariantCulture);
    }

    // Parse font details from an inline style string.
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
