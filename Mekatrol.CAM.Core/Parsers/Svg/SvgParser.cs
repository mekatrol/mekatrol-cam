using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public class SvgParser(ILogger<SvgParser> logger) : SvgParserBase(logger), ISvgParser
{
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

        var viewEntity = ParseSvgElement(xmlDocument.Root);

        // Translate path from (minx, miny) to (0, 0)
        if (translateToZero)
        {
            viewEntity.TranslateLocation(-viewEntity.Location);
        }

        return viewEntity;
    }

    private IGeometricPathEntity ParseSvgElement(XElement element)
    {
        AssertIsTag(element, "svg");

        var viewBoxAttr = GetAttributeValue(element, "viewBox");

        var location = new PointDouble(0, 0);
        var size = new PointDouble(0, 0);
        var defaultViewPort = true;

        var rx = new Regex(@"viewBox\s*=\s*""\s*(?<minx>-?\d+(?:\.\d+)?)\s+(?<miny>-?\d+(?:\.\d+)?)\s+(?<width>\d+(?:\.\d+)?)\s+(?<height>\d+(?:\.\d+)?)\s*""", RegexOptions.IgnoreCase);

        var m = rx.Match(viewBoxAttr ?? string.Empty);
        if (m.Success)
        {
            var minX = double.Parse(m.Groups["minx"].Value);
            var minY = double.Parse(m.Groups["miny"].Value);
            var width = double.Parse(m.Groups["width"].Value);
            var height = double.Parse(m.Groups["height"].Value);

            // Setting view point, don't use default
            defaultViewPort = false;

            location = new PointDouble(minX, minY);
            size = new PointDouble(width, height);
        }

        var entities = ParseSvgElementChildren(element).ToList();

        // Determine view port from min and max values of all enities in the view
        var min = new PointDouble(0, 0);
        var max = new PointDouble(0, 0);

        foreach (var entity in entities)
        {
            // Create untransformed and transformed points and boundaries
            entity.InitializeState(GeometryTransform.Identity);

            // Update min/max from entity
            min = min.Min(entity.MinUntransformed);
            max = max.Max(entity.MaxUntransformed);
        }

        if (defaultViewPort)
        {
            location = min;
            size = max - min;
        }

        var viewEntity = new ViewEntity(location, size, entities, GeometryTransform.Identity);
        viewEntity.UpdateBoundary();

        return viewEntity;
    }

    private IGeometricEntity ParseGElement(XElement element)
    {
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

                case "svg":
                    {
                        var childSvg = ParseSvgElement(element);
                        geometries.AddRange(childSvg.Entities); // flatten nested <svg>
                        break;
                    }

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
                    geometries.Add(new TextParser(_logger).ParseTextElement(element));
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
                if (pointValues.Length != 2) { throw exception; }

                if (!double.TryParse(pointValues[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                    !double.TryParse(pointValues[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                {
                    throw exception;
                }

                points.Add(new PointDouble(x, y));
            }

            // Switch between point and space
            isPoint = !isPoint;
        }

        if (offset != pointsAttribute.Value.Length) { throw exception; }

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
        var matches = Regex.Matches(pointsAttribute.Value, pointsPattern).Cast<Match>().ToList();

        var lineInfo = (IXmlLineInfo)element;
        var exception = new XmlException("Invalid polyline points value.", null, lineInfo.LineNumber, lineInfo.LinePosition);

        var points = new List<PointDouble>();

        var offset = 0;
        var isPoint = true;
        foreach (var match in matches)
        {
            if (match.Index != offset) { throw exception; }
            offset += match.Length;

            if (!isPoint)
            {
                if (!string.IsNullOrWhiteSpace(match.Value) && match.Value != ",") { throw exception; }
            }
            else
            {
                var pointValues = match.Value.Split(',');
                if (pointValues.Length != 2) { throw exception; }

                if (!double.TryParse(pointValues[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
                    !double.TryParse(pointValues[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                {
                    throw exception;
                }

                points.Add(new PointDouble(x, y));
            }

            // Switch between point and space
            isPoint = !isPoint;
        }

        if (offset != pointsAttribute.Value.Length) { throw exception; }

        return new PolylineEntity(points, ParseTransformAttribute(element));
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
            var (startLocation, geometries, closed) = new SvgPathParser(pathAttribute.Value).Parse();
            var first = geometries.FirstOrDefault();
            return new PathEntity(first?.Location.X ?? 0, first?.Location.Y ?? 0, geometries, closed, new GeometryTransform());
        }
        catch (Exception ex)
        {
            var lineInfo = (IXmlLineInfo)element;
            throw new XmlException("Invalid path.", ex, lineInfo.LineNumber, lineInfo.LinePosition);
        }
    }

    private void ParseStyleElement(XElement element)
    {
        var css = element.Value;
        _cssClasses = CssParser.Parse(css);
    }
}
