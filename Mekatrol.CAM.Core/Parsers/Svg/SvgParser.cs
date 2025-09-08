using Avalonia.Media;
using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public class SvgParser : ISvgParser
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

        var fontFamily = RenderExtensions.BestFontFamily(RenderExtensions.DefaultFontFamilyName);
        _currentFont = new FontDescription(fontFamily.Name, 30, FontStyle.Normal, FontWeight.Normal);
    }

    public IGeometricPathEntity Parse(StreamReader stream, bool translateToZero = false)
    {
        if (stream == null || stream.EndOfStream)
        {
            return new PathEntity();
        }

        var xmlDocument = XDocument.Load(stream, LoadOptions.SetLineInfo);

        if (xmlDocument.Root == null)
        {
            return new PathEntity();
        }

        var pathEntity = ParseSvgElement(xmlDocument.Root);

        // Create untransformed andtransformed points and boundaries
        pathEntity.InitializeState(GeometryTransform.Identity);

        // Translate path from (minx, miny) to (0, 0)
        if (translateToZero)
        {
            var translate = new PointDouble();

            foreach (var entity in pathEntity.Entities)
            {
                // Update translate to minimum of location and current translate value 
                translate = translate.Min(entity.Location);
            }

            foreach (var entity in pathEntity.Entities)
            {
                entity.TranslateLocation(translate);
            }
        }

        return pathEntity;
    }

    private IGeometricPathEntity ParseSvgElement(XElement element)
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
                var values = parts.Select(double.Parse).ToArray();
            }
        }

        return new PathEntity(0, 0, ParseSvgElementChildren(element).ToList(), false, GeometryTransform.Identity);
    }

    private IGeometricEntity ParseGElement(XElement element)
    {
        // We are expecting the svg tag
        AssertIsTag(element, "g");

        var transform = ParseTransformAttribute(element);

        var children = ParseSvgElementChildren(element);

        var path = new PathEntity(0, 0, children.ToList(), false, transform);

        return path;
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
                    geometries.Add(ParseGElement(element));
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
        else
        {
            // ry is null, so default to rx
            ry ??= rx;
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

        List<PointDouble> points = [];

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

        return new PolylineEntity(points.AsReadOnly<PointDouble>(), ParseTransformAttribute(element));
    }

    private static IGeometricEntity ParsePathElement(XElement element)
    {
        AssertIsTag(element, "path");

        var pathAttribute = element.Attribute("d");
        if (pathAttribute == null || string.IsNullOrWhiteSpace(pathAttribute.Value))
        {
            // No 'd' attribute value so return empty path
            return new PathEntity(0, 0, [], false, new GeometryTransform());
        }

        try
        {
            // Parse the path to a set of geometries
            var (startLocation, geometries, closed) = new SvgPathParser(pathAttribute.Value).Parse();

            // Keep path container transform identity after baking
            var first = geometries.FirstOrDefault();
            return new PathEntity(first?.Location.X ?? 0, first?.Location.Y ?? 0, geometries, closed, new GeometryTransform());
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
        var transform = ParseTransformAttribute(element);

        var x = xElement;
        var y = yElement;

        var textAlign = TextAlignment.Left;
        var textAnchor = GetAttributeValue(element, "text-anchor");
        if (textAnchor != null)
        {
            textAlign = textAnchor.ToLower() switch
            {
                "middle" => TextAlignment.Center,
                "end" => TextAlignment.Right,
                // "start"
                _ => TextAlignment.Left,
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
                        var textEntity = new TextEntity(x, y, text.Trim(), font, textAlign, new GeometryTransform());
                        childText.Add(textEntity);
                        var spaceSize = GeometryUtils.MeasureText("I", textEntity.Font, textAlign, 0, 0, Matrix3.Identity);
                        x += textEntity.BoundaryUntransformed.Size.X + spaceSize.X;
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
                        x += textEntity.BoundaryUntransformed.Size.X + spaceSize.X;
                    }
                    else if (texts.Type == GeometricEntityType.Path)
                    {
                        var path = (PathEntity)texts;

                        if (path.Entities.Count == 0)
                        {
                            continue;
                        }

                        var spaceSize = GeometryUtils.MeasureText("I", font, textAlign, 0, 0, Matrix3.Identity);
                        x += path.BoundaryUntransformed.Size.X + spaceSize.X;

                        childText.AddRange(path.Entities);
                    }
                    break;

                default:
                    continue;
            }
        }

        var entityPath = new PathEntity(xElement, yElement, childText, false, transform);
        return entityPath;
    }

    private IGeometricEntity ParseTextElement(XElement element)
    {
        AssertIsTag(element, "text");

        var x = GetAttributeDoubleValue(element, "x").Value ?? 0.0;
        var y = GetAttributeDoubleValue(element, "y").Value ?? 0.0;

        // Extract text elements as entity path
        var textPath = (PathEntity)ParseTextSubElement(element, "text", _currentFont, x, y);

        return new PathEntity(textPath.Location.X, textPath.Location.Y, textPath.Entities, /*closed:*/ false, ParseTransformAttribute(element));
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

    private static GeometryTransform ParseTransformAttribute(XElement element)
    {
        var transformAttr = GetAttributeValue(element, "transform")?.Trim();
        var transform = new GeometryTransform();

        if (string.IsNullOrWhiteSpace(transformAttr))
        {
            return new GeometryTransform();
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

            transformDefinitions.Add(transformAttr[..(closeBracketIndex + 1)].Trim());
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

                transform.Rotate += new GeometryRotate(GeometryUtils.RadiansToDegrees(m.GetRotation()), 0.0, 0.0);
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
        int expectedArrayLength)
    {
        // A matrix transform has 6 values
        var openBracketIndex = value.IndexOf('(');
        var closeBracketIndex = value.IndexOf(')');

        // Invalid syntax?
        if (openBracketIndex < 0 || closeBracketIndex < 0)
        {
            return Enumerable.Repeat(defaultValue, expectedArrayLength).ToArray();
        }

        // Split by comma or space
        var raw = value[(openBracketIndex + 1)..closeBracketIndex];
        var values = raw.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);

        var doubleValues = values
            .Select(x => double.TryParse(x, out var v) ? v : defaultValue)
            .ToList();

        // Make sure we have at least expected array length
        var last = doubleValues.Count > 0 ? doubleValues[^1] : defaultValue;
        while (doubleValues.Count < expectedArrayLength)
        {
            doubleValues.Add(copyRight ? last : defaultValue);
        }

        // Take at most expected array length
        return doubleValues.Take(expectedArrayLength).ToArray();
    }

    // SVG matrix(a, b, c, d, e, f) represents the 3x3 affine matrix:
    // [ a  c  e ]
    // [ b  d  f ]
    // [ 0  0  1 ]
    //
    // Applied to a column vector [x y 1]^T:
    //   x' = a*x + c*y + e
    //   y' = b*x + d*y + f
    //
    // Intuition:
    //   a,d : primary scale and rotation terms (cosθ*scale, etc.)
    //   b,c : cross terms: rotation and shear
    //   e,f : translation in X and Y
    //
    // Notes:
    //   - Pure scale(sx, sy): a=sx, d=sy, b=c=0, e=f=0
    //   - Pure rotate(θ):    a=cosθ, c=-sinθ, b=sinθ, d=cosθ
    //   - Pure shear(shx,shy): c=shx, b=shy
    //   - Pure translate(tx,ty): e=tx, f=ty
    //   - Negative scales flip axes.
    private static Matrix3 ParseMatrix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Matrix3.Identity;
        }

        // Parse a,b,c,d,e,f from "matrix(a,b,c,d,e,f)"
        var values = ParseBracketValues(value, copyRight: false, defaultValue: 0.0, expectedArrayLength: 6);

        // Matrix3 expects row-major storage with layout:
        // [ m0 m1 m2 ]
        // [ m3 m4 m5 ]
        // [ m6 m7 m8 ]
        //
        // 2D block is:
        // [ a b c ]
        // [ d e f ]
        // with bottom row [0 0 1].
        //
        // Therefore map SVG → internal:
        // SVG [ a  c  e ]      internal row 0: [ a  c  e ] -> m[0], m[1], m[2]
        //     [ b  d  f ]  →   internal row 1: [ b  d  f ] -> m[3], m[4], m[5]
        //     [ 0  0  1 ]      internal row 2: [ 0  0  1 ] -> m[6], m[7], m[8]

        var m = new double[9];

        // m[0] = a : scales X, and contributes cosθ for rotation around origin.
        m[0] = values[0]; // a

        // m[1] = c : mixes Y into X; rotation (−sinθ) and X-shear component.
        m[1] = values[2]; // c

        // m[2] = e : translation in X (moves all points horizontally).
        m[2] = values[4]; // e

        // m[3] = b : mixes X into Y; rotation (sinθ) and Y-shear component.
        m[3] = values[1]; // b

        // m[4] = d : scales Y, and contributes cosθ for rotation around origin.
        m[4] = values[3]; // d

        // m[5] = f : translation in Y (moves all points vertically).
        m[5] = values[5]; // f

        // Homogeneous bottom row for affine transforms.
        m[6] = 0; // no projective terms
        m[7] = 0; // no projective terms
        m[8] = 1; // homogeneous scale

        return new Matrix3(m);
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

    private static GeometryRotate ParseRotate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new GeometryRotate();
        }

        var values = ParseBracketValues(value, false, 0.0, 3);
        return new GeometryRotate(values[0], values[1], values[2]);
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
