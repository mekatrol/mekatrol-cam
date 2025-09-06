using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Mekatrol.CAM.Core.Parsers.Svg;

internal class SvgParser : ISvgParser
{
    internal const string DoublePattern = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
    internal const string DoublePairPattern = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)\s*,{0,1}\s*([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
    internal const string DoubleWithUnitsPattern = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)([A-Za-z%]*)";
    private readonly ILogger _logger;
    private readonly FontDescription _currentFont;

    private IDictionary<string, CssClass> _cssClasses = new Dictionary<string, CssClass>(StringComparer.OrdinalIgnoreCase);

    public SvgParser(ILogger<SvgParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var fontFamily = GraphicsExtensions.BestFontFamily(GraphicsExtensions.DefaultFontFamilyName);
        _currentFont = new FontDescription(fontFamily.Name, 30, FontStyle.Regular);
    }

    public IReadOnlyList<IGeometricEntity> Parse(StreamReader stream, bool translateToZero = false)
    {
        if (stream == null || stream.EndOfStream)
        {
            return [];
        }

        var xmlDocument = XDocument.Load(stream, LoadOptions.SetLineInfo);

        if (xmlDocument.Root == null)
        {
            return [];
        }

        var entities = ParseSvgElement(xmlDocument.Root);

        if (translateToZero)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;

            foreach (var entity in entities)
            {
                var (min, max) = entity.GetMinMax();

                minX = Math.Min(minX, min.X);
                minY = Math.Min(minY, min.Y);
            }

            var translate = new PointDouble(-minX, -minY);

            // Shift the entities so that the first sits in the very top left
            var translateMatrix = Matrix3.CreateTranslate(translate);

            foreach (var entity in entities)
            {
                entity.TransformBy(translateMatrix);
            }
        }

        return entities;
    }

    private IReadOnlyList<IGeometricEntity> ParseSvgElement(XElement element)
    {
        // We are expecting the svg tag
        AssertIsTag(element, "svg");

        // Get view box
        var viewBox = GetAttributeValue(element, "viewBox");

        if (!string.IsNullOrWhiteSpace(viewBox))
        {
            var parts = viewBox.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                var values = parts.Select(x => double.Parse(x)).ToArray();
            }
        }

        return ParseSvgElementChildren(element);
    }

    private IReadOnlyList<IGeometricEntity> ParseGElement(XElement element)
    {
        // We are expecting the svg tag
        AssertIsTag(element, "g");

        return ParseSvgElementChildren(element);
    }

    private IReadOnlyList<IGeometricEntity> ParseSvgElementChildren(XElement parentElement)
    {
        // The list of geometries to be returned
        var geometries = new List<IGeometricEntity>();

        foreach (var element in parentElement.Elements())
        {
            var lineInfo = (IXmlLineInfo)element;
            switch (element.Name.LocalName.ToLowerInvariant())
            {
                // The g element can contain multiple svg elements
                case "g":
                    geometries.AddRange(ParseGElement(element));
                    break;

                // The svg element can be muliple depths
                case "svg":
                    geometries.AddRange(ParseSvgElement(element));
                    break;

                case "circle":
                    geometries.Add(ParseCircleElement(element));
                    break;

                case "ellipse":
                    geometries.Add(ParseEllipseElement(element));
                    break;

                case "line":
                    geometries.Add(ParseLineElement(element));
                    break;

                case "path":
                    geometries.Add(ParsePathElement(element));
                    break;

                case "polygon":
                    geometries.Add(ParsePolygonElement(element));
                    break;

                case "polyline":
                    geometries.Add(ParsePolylineElement(element));
                    break;

                case "rect":
                    geometries.Add(ParseRectElement(element));
                    break;

                case "text":
                    geometries.Add(ParseTextElement(element));
                    break;

                case "style":
                    ParseStyleElement(element);
                    break;

                // These are valid elements for an SVG, but we ignore them
                case "a":
                case "animate":
                case "animatetransform":
                case "audio":
                case "defs":
                case "desc":
                case "image":
                case "lineargradient":
                case "metadata":
                case "namedview":
                case "pattern":
                case "radialgradient":
                case "script":
                case "set":
                case "svgtestcase":
                case "title":
                case "use":
                case "video":
                    continue;

                default:
                    _logger.LogWarning("Invalid element <{ElementName}> ({LineNumber}, {LinePosition})", element.Name.LocalName, lineInfo.LineNumber, lineInfo.LinePosition);
                    continue;
            }
        }

        return geometries;
    }

    private static IGeometricEntity ParseRectElement(XElement element)
    {
        AssertIsTag(element, "rect");

        var x = GetAttributeDoubleValue(element, "x").Value ?? 0.0;
        var y = GetAttributeDoubleValue(element, "y").Value ?? 0.0;
        var w = GetAttributeDoubleValue(element, "width").Value ?? 1.0;
        var h = GetAttributeDoubleValue(element, "height").Value ?? 1.0;
        var rx = GetAttributeDoubleValue(element, "rx").Value;
        var ry = GetAttributeDoubleValue(element, "ry").Value;

        if (rx == null && ry == null)
        {
            // Both are null, so default to zero
            rx = ry = 0.0;
        }
        else if (rx == null)
        {
            // rx is null, so default to ry
            rx = ry;
        }
        else if (ry == null)
        {
            // ry is null, so default to rx
            ry = rx;
        }

        return new RectangleEntity(x, y, w, h, rx ?? 0.0, ry ?? 0.0, ParseTransformAttribute(element));
    }

    private static IGeometricEntity ParseCircleElement(XElement element)
    {
        AssertIsTag(element, "circle");

        var x = GetAttributeDoubleValue(element, "cx").Value ?? 0.0;

        var y = GetAttributeDoubleValue(element, "cy").Value ?? 0.0;

        // r is not optional for a circle
        var r = GetAttributeDoubleValue(element, "r").Value;

        var lineInfo = (IXmlLineInfo)element;

        if (r == null)
        {
            throw new XmlException("circle 'r' attribute must be provided", null, lineInfo.LineNumber, lineInfo.LinePosition);
        }

        return new CircleEntity(x, y, r.Value, ParseTransformAttribute(element));
    }

    private static IGeometricEntity ParseEllipseElement(XElement element)
    {
        AssertIsTag(element, "ellipse");

        var x = GetAttributeDoubleValue(element, "cx").Value ?? 0.0;
        var y = GetAttributeDoubleValue(element, "cy").Value ?? 0.0;
        var rx = GetAttributeDoubleValue(element, "rx").Value ?? 1.0;
        var ry = GetAttributeDoubleValue(element, "ry").Value ?? 1.0;

        return new EllipseEntity(x, y, rx, ry, ParseTransformAttribute(element));
    }

    private static IGeometricEntity ParseLineElement(XElement element)
    {
        AssertIsTag(element, "line");

        var x1 = GetAttributeDoubleValue(element, "x1").Value ?? 0.0;
        var y1 = GetAttributeDoubleValue(element, "y1").Value ?? 0.0;
        var x2 = GetAttributeDoubleValue(element, "x2").Value ?? 0.0;
        var y2 = GetAttributeDoubleValue(element, "y2").Value ?? 0.0;

        return new LineEntity(x1, y1, x2, y2, ParseTransformAttribute(element));
    }

    private static IGeometricEntity ParsePolygonElement(XElement element)
    {
        AssertIsTag(element, "polygon");

        var pointsAttribute = element.Attribute("points");

        // If the points attribute is missing or empty then return empty polygon
        if (pointsAttribute == null || string.IsNullOrWhiteSpace(pointsAttribute.Value))
        {
            return new PolygonEntity();
        }

        // Make sure that the points match the pattern for points
        const string pointsPattern = @$"({DoublePairPattern})|(\s+|,+)";
        var matches = Regex
            .Matches(pointsAttribute.Value, pointsPattern)
            .Cast<Match>()
            .ToList();

        var lineInfo = (IXmlLineInfo)element;
        var exception = new XmlException("Invalid polygon points value.", null, lineInfo.LineNumber, lineInfo.LinePosition);

        var points = new List<PointDouble>();

        var offset = 0;
        var isPoint = true;
        foreach (var match in matches)
        {
            if (match.Index != offset)
            {
                throw exception;
            }

            offset += match.Length;

            if (!isPoint)
            {
                if (!string.IsNullOrWhiteSpace(match.Value) && match.Value != ",")
                {
                    throw exception;
                }
            }
            else
            {
                var pointValues = match.Value.Split(',');
                if (pointValues.Length != 2)
                {
                    throw exception;
                }

                if (!double.TryParse(pointValues[0], out var x) ||
                    !double.TryParse(pointValues[1], out var y))
                {
                    throw exception;
                }

                points.Add(new PointDouble(x, y));
            }

            // Switch between point and space
            isPoint = !isPoint;
        }

        if (offset != pointsAttribute.Value.Length)
        {
            throw exception;
        }

        return new PolygonEntity(points, ParseTransformAttribute(element));
    }

    private static IGeometricEntity ParsePolylineElement(XElement element)
    {
        AssertIsTag(element, "polyline");

        var pointsAttribute = element.Attribute("points");

        // If the points attribute is missing or empty then return empty polygon
        if (pointsAttribute == null || string.IsNullOrWhiteSpace(pointsAttribute.Value))
        {
            return new PolylineEntity();
        }

        // Make sure that the points match the pattern for points
        const string pointsPattern = @$"({DoublePairPattern})|(\s+|,+)";
        var matches = Regex
            .Matches(pointsAttribute.Value, pointsPattern)
            .Cast<Match>()
            .ToList();

        var lineInfo = (IXmlLineInfo)element;
        var exception = new XmlException("Invalid polyline points value.", null, lineInfo.LineNumber, lineInfo.LinePosition);

        var points = new List<PointDouble>();

        var offset = 0;
        var isPoint = true;
        foreach (var match in matches)
        {
            if (match.Index != offset)
            {
                throw exception;
            }

            offset += match.Length;

            if (!isPoint)
            {
                if (!string.IsNullOrWhiteSpace(match.Value) && match.Value != ",")
                {
                    throw exception;
                }
            }
            else
            {
                var pointValues = match.Value.Split(',');
                if (pointValues.Length != 2)
                {
                    throw exception;
                }

                if (!double.TryParse(pointValues[0], out var x) ||
                    !double.TryParse(pointValues[1], out var y))
                {
                    throw exception;
                }

                points.Add(new PointDouble(x, y));
            }

            // Switch between point and space
            isPoint = !isPoint;
        }

        if (offset != pointsAttribute.Value.Length)
        {
            throw exception;
        }

        return new PolylineEntity(points, ParseTransformAttribute(element));
    }

    private static IGeometricEntity ParsePathElement(XElement element)
    {
        AssertIsTag(element, "path");

        var pathAttribute = element.Attribute("d");

        if (pathAttribute == null || string.IsNullOrWhiteSpace(pathAttribute.Value))
        {
            // No 'd' attribute value so return empty path
            return new PathEntity(0, 0, [], false, ParseTransformAttribute(element));
        }

        try
        {
            // Parse the path to a set of geometries
            var (startLocation, geometries, closed) = new SvgPathParser(pathAttribute.Value).Parse();
            var first = geometries.FirstOrDefault();

            return new PathEntity(first?.Location.X ?? 0, first?.Location.Y ?? 0, geometries, closed, ParseTransformAttribute(element));
        }
        catch (Exception ex)
        {
            var lineInfo = (IXmlLineInfo)element;
            throw new XmlException("Invalid path.", ex, lineInfo.LineNumber, lineInfo.LinePosition);
        }
    }

    private IGeometricEntity ParseTextSubElement(
        XElement element,
        string tag,
        FontDescription? parentFont,
        double parentX,
        double parentY)
    {
        AssertIsTag(element, tag);

        var xElement = GetAttributeDoubleValue(element, "x").Value ?? parentX;
        var yElement = GetAttributeDoubleValue(element, "y").Value ?? parentY;

        var x = xElement;
        var y = yElement;

        var textAlign = StringAlignment.Near;
        var textAnchor = GetAttributeValue(element, "text-anchor");
        if (textAnchor != null)
        {
            textAlign = textAnchor.ToLower() switch
            {
                "middle" => StringAlignment.Center,
                "end" => StringAlignment.Far,
                // "start"
                _ => StringAlignment.Near,
            };
        }

        // The initial font is the parent font if passed, else the default font
        var font = parentFont ?? _currentFont;

        // Use the globally defined css classes to define font if exists
        var className = GetAttributeValue(element, "class");
        if (className != null && _cssClasses.TryGetValue(className, out var cssFontClass))
        {
            if (cssFontClass != null && cssFontClass.Font != null)
            {
                font = cssFontClass.Font;
            }
        }

        // The text element may have its own font defined
        var style = GetAttributeValue(element, "style");
        var parsedFont = ParseFontFromStyle(style);
        if (parsedFont != null)
        {
            font = parsedFont;
        }

        // The text element may have a font size
        var fontSizeValue = GetAttributeValue(element, "font-size");
        if (fontSizeValue != null)
        {
            CssParser.ExtractFontSize(fontSizeValue, font);
        }

        // The text in the element may be raw (text directly in value)
        // or part of a tspan child element
        var childNodes = element.Nodes().ToList();
        var childText = new List<IGeometricEntity>();

        foreach (var childNode in childNodes)
        {
            switch (childNode.NodeType)
            {
                case XmlNodeType.CDATA:
                case XmlNodeType.Text:
                    {
                        // Get text
                        var text = (childNode as XText)!.Value;

                        if (string.IsNullOrWhiteSpace(text))
                        {
                            continue;
                        }

                        // Create text child
                        var textEntity = new TextEntity(x, y, text.Trim(), font, textAlign, new Transform());
                        childText.Add(textEntity);
                        var spaceSize = GeometryUtils.MeasureText("I", textEntity.Font, textAlign, 0, 0, Matrix3.Identity);
                        x += textEntity.Boundary.Size.X + spaceSize.X;
                    }
                    break;

                case XmlNodeType.Element:

                    var childElement = (XElement)childNode;

                    // Only process TSpans
                    if (childElement.Name.LocalName != "tspan")
                    {
                        continue;
                    }

                    // Parse text span as text elements
                    var texts = ParseTextSubElement(childElement, "tspan", font, x, y);
                    if (texts.Type == GeometricEntityType.Text)
                    {
                        var textEntity = (TextEntity)texts;
                        childText.Add(textEntity);
                        var spaceSize = GeometryUtils.MeasureText("I", textEntity.Font, textAlign, 0, 0, Matrix3.Identity);
                        x += textEntity.Boundary.Size.X + spaceSize.X;
                    }
                    else if (texts.Type == GeometricEntityType.Path)
                    {
                        var path = (PathEntity)texts;

                        if (path.Entities.Count == 0)
                        {
                            continue;
                        }

                        var spaceSize = GeometryUtils.MeasureText("I", font, textAlign, 0, 0, Matrix3.Identity);
                        x += path.Boundary.Size.X + spaceSize.X;

                        childText.AddRange(path.Entities);
                    }
                    break;

                default:
                    continue;
            }
        }

        var entityPath = new PathEntity(xElement, yElement, childText, false, new Transform());
        return entityPath;
    }

    private IGeometricEntity ParseTextElement(XElement element)
    {
        AssertIsTag(element, "text");

        var x = GetAttributeDoubleValue(element, "x").Value ?? 0.0;
        var y = GetAttributeDoubleValue(element, "y").Value ?? 0.0;

        // Get the element transform
        var transform = ParseTransformAttribute(element);

        // Extract text elements as entity path
        var textPath = (PathEntity)ParseTextSubElement(element, "text", _currentFont, x, y);

        // Transform the text paths (this is mostly rotating)
        textPath.TransformBy(transform.GetMatrix());

        return textPath;
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

    private void ParseStyleElement(XElement element)
    {
        var css = element.Value;
        _cssClasses = CssParser.Parse(css);
    }

    private static ITransform ParseTransformAttribute(XElement element)
    {
        var transformAttr = GetAttributeValue(element, "transform")?.Trim();
        var transform = new Transform();

        if (string.IsNullOrWhiteSpace(transformAttr))
        {
            return new Transform();
        }

        var transformDefinitions = new List<string>();

        while (transformAttr.Length > 0)
        {
            var closeBracketIndex = transformAttr.IndexOf(')');
            if (closeBracketIndex < 0)
            {
                // A closing bracket should always be found
                // when there are characters left
                return transform;
            }

            transformDefinitions.Add(transformAttr[..(closeBracketIndex + 1)]);
            transformAttr = transformAttr[(closeBracketIndex + 1)..];
        }

#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var transformOrigin = ParseTransformOrigin(element);
#pragma warning restore IDE0059 // Unnecessary assignment of a value

        // A negative scale flips the geometry
        var flipX = false;
        var flipY = false;

        // Transforms are commutative in that it can repeat element types
        // so we process from left to right
        foreach (var transformDefinition in transformDefinitions)
        {
            // A transform can be:
            //  a SVG transform: https://developer.mozilla.org/en-US/docs/Web/SVG/Attribute/transform
            //  a CSS transform: https://developer.mozilla.org/en-US/docs/Web/CSS/transform
            // **** WE DONT PROCESS CSS TRANSFORMS, ONLY SVG ONES

            // Given a transform can have multiple types of definition we prioritise to use the matrix
            // definition over scale, translate and rotate. Technically it should have matrix or other types
            // but not both.
            if (transformDefinition.StartsWith("matrix", StringComparison.OrdinalIgnoreCase))
            {
                var m = ParseMatrix(transformDefinition);

                transform.Rotate += new Rotate(GeometryUtils.RadiansToDegrees(m.GetRotation()), 0.0, 0.0);
                transform.Scale *= m.GetScale();
                transform.Translate += m.GetTranslation();
            }
            else if (transformDefinition.StartsWith("scale", StringComparison.OrdinalIgnoreCase))
            {
                var scale = ParseScale(transformDefinition);

                if (scale.X < 0)
                {
                    flipX = !flipX;
                    scale.X = -scale.X;
                }

                if (scale.Y < 0)
                {
                    flipY = !flipY;
                    scale.Y = -scale.Y;
                }

                transform.Scale *= scale;
            }
            else if (transformDefinition.StartsWith("rotate", StringComparison.OrdinalIgnoreCase))
            {
                var rotation = ParseRotate(transformDefinition);
                transform.Rotate.Angle += rotation.Angle;
            }
            else if (transformDefinition.StartsWith("translate", StringComparison.OrdinalIgnoreCase))
            {
                var translation = ParseTranslate(transformDefinition);
                transform.Translate += translation;
            }
            else if (transformDefinition.StartsWith("skewx", StringComparison.OrdinalIgnoreCase))
            {
                var skew = ParseSkewX(transformDefinition);
                transform.SkewX *= skew;
            }
            else if (transformDefinition.StartsWith("skewy", StringComparison.OrdinalIgnoreCase))
            {
                var skew = ParseSkewY(transformDefinition);
                transform.SkewY *= skew;
            }
        }

        return transform;
    }

    private static PointDouble ParseTransformOrigin(XElement element)
    {
        // Get transform origin if exists
        var transformOriginValue = GetAttributeValue(element, "transform-origin")?.Trim() ?? "0 0";
        var transformOriginValues = transformOriginValue
            .Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(x =>
            {
                if (double.TryParse(x, out var value))
                {
                    return value;
                }

                return 0.0;
            })
            .ToList();

        while (transformOriginValues.Count < 2)
        {
            transformOriginValues.Add(0.0);
        }

        return new PointDouble(transformOriginValues[0], transformOriginValues[1]);
    }

    private static IList<double> ParseBracketValues(
        string value,
        bool copyRight,
        double defaultValue,
        int expectedArrayLength,
        char separator = ' ')
    {
        // A matrix transform has 6 values
        var openBracketIndex = value.IndexOf('(');
        var closeBracketIndex = value.IndexOf(')');

        // Invalid syntax?
        if (openBracketIndex < 0 || closeBracketIndex < 0)
        {
            return Enumerable
                .Range(0, expectedArrayLength)
                .Select(x => defaultValue)
                .ToArray();
        }

        var values = value[(openBracketIndex + 1)..closeBracketIndex]
            .Split([separator], StringSplitOptions.RemoveEmptyEntries);

        var doubleValues = values
            .Select(x =>
            {
                if (double.TryParse(x, out var v))
                {
                    return v;
                }
                else
                {
                    return defaultValue;
                }
            })
            .ToList();

        // Make sure we have at least expected array length
        var last = doubleValues.Count > 0 ? doubleValues.Last() : defaultValue;
        while (doubleValues.Count < expectedArrayLength)
        {
            doubleValues.Add(copyRight ? last : defaultValue);
        }

        // Take at most expected array length
        return doubleValues.Take(expectedArrayLength).ToArray();
    }

    private static Matrix3 ParseMatrix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Matrix3.Identity;
        }

        var values = ParseBracketValues(value, false, 0.0, 6, ',');

        var m = new double[9];
        // Transpose cols / array
        // SVG has structure:
        // [a c e]
        // [b d f]

        // But our matrix structure is:
        // [a b c]
        // [d e f]

        m[0] = values[0];
        m[1] = values[2];
        m[2] = values[4];
        m[3] = values[1];
        m[4] = values[3];
        m[5] = values[5];

        // Add bottom row
        m[6] = 0;
        m[7] = 0;
        m[8] = 1;

        var matrix = new Matrix3(m);

        return matrix;
    }

    private static PointDouble ParseTranslate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new PointDouble(0, 0);
        }

        var values = ParseBracketValues(value, false, 0.0, 2);
        return new PointDouble(values[0], values[1]);
    }

    private static PointDouble ParseScale(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new PointDouble(0, 0);
        }

        var values = ParseBracketValues(value, true, 1.0, 2);
        return new PointDouble(values[0], values[1]);
    }

    private static Rotate ParseRotate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new Rotate();
        }

        var values = ParseBracketValues(value, false, 0.0, 3);
        return new Rotate(values[0], values[1], values[2]);
    }

    private static double ParseSkewX(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0.0;
        }

        var values = ParseBracketValues(value, false, 0.0, 1);
        return values[0];
    }

    private static double ParseSkewY(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0.0;
        }

        var values = ParseBracketValues(value, false, 0.0, 1);
        return values[0];
    }

    private static void AssertIsTag(XElement element, string tagName)
    {
        var lineInfo = (IXmlLineInfo)element;
        if (!element.Name.LocalName.Equals(tagName, StringComparison.OrdinalIgnoreCase))
        {
            throw new XmlException($"Expecting <{tagName}> element", null, lineInfo.LineNumber, lineInfo.LinePosition);
        }
    }

    private static (double? Value, string? Units) GetAttributeDoubleValue(XElement element, string attributeName)
    {
        var attribute = element.Attribute(attributeName);
        if (attribute == null)
        {
            return (null, null);
        }

        var textValue = attribute.Value?.Trim();
        if (textValue == null)
        {
            return (null, null);
        }

        var match = Regex.Match(textValue, DoubleWithUnitsPattern);
        if (!match.Success || match.Length != textValue.Length)
        {
            var lineInfo = (IXmlLineInfo)element;
            throw new XmlException($"{element.Name.LocalName} '{attributeName}' attribute must be a valid floating point number", null, lineInfo.LineNumber, lineInfo.LinePosition);
        }

        var value = double.Parse(match.Groups[1].Value);
        var units = match.Groups[2].Value;

        if (units.Trim().Length == 0)
        {
            units = null;
        }

        return (value, units);
    }

    private static string? GetAttributeValue(XElement element, string attributeName)
    {
        var attribute = element.Attribute(attributeName);
        if (attribute == null)
        {
            return null;
        }

        var textValue = attribute.Value?.Trim();
        if (textValue == null)
        {
            return null;
        }

        return textValue;
    }
}
