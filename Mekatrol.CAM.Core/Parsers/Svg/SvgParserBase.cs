using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Render;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Mekatrol.CAM.Core.Parsers.Svg;

public abstract class SvgParserBase(ILogger logger)
{
    internal const string DoublePattern = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
    internal const string DoublePairPattern = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)\s*,{0,1}\s*([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)";
    internal const string DoubleWithUnitsPattern = @"([-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?)([A-Za-z%]*)";

    protected readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected IDictionary<string, CssClass> _cssClasses = new Dictionary<string, CssClass>(StringComparer.OrdinalIgnoreCase);

    public static void AssertIsTag(XElement element, string tagName)
    {
        var lineInfo = (IXmlLineInfo)element;
        if (!element.Name.LocalName.Equals(tagName, StringComparison.OrdinalIgnoreCase))
        {
            throw new XmlException($"Expecting <{tagName}> element", null, lineInfo.LineNumber, lineInfo.LinePosition);
        }
    }

    protected static (double? Value, string? Units) GetAttributeDoubleValue(XElement element, string attributeName)
    {
        var attribute = element.Attribute(attributeName);
        if (attribute == null) { return (null, null); }

        var textValue = attribute.Value?.Trim();
        if (textValue == null) { return (null, null); }

        var match = Regex.Match(textValue, DoubleWithUnitsPattern);
        if (!match.Success || match.Length != textValue.Length)
        {
            var lineInfo = (IXmlLineInfo)element;
            throw new XmlException($"{element.Name.LocalName} '{attributeName}' attribute must be a valid floating point number", null, lineInfo.LineNumber, lineInfo.LinePosition);
        }

        var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var units = match.Groups[3].Value; // units are group 3

        if (string.IsNullOrWhiteSpace(units)) { units = null; }

        return (value, units);
    }

    protected static string? GetAttributeValue(XElement element, string attributeName)
    {
        var attribute = element.Attribute(attributeName);
        if (attribute == null) { return null; }

        var textValue = attribute.Value?.Trim();
        if (textValue == null) { return null; }

        return textValue;
    }

    protected static GeometryTransform ParseTransformAttribute(XElement element)
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

        var _ = ParseTransformOrigin(element);

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

            var regex = new Regex(@"^(?<attr>matrix|scale|rotate|translate|skewx|skewy)(?![A-Za-z0-9_])", RegexOptions.IgnoreCase);

            var match = regex.Match(transformDefinition);

            if (!match.Success)
            {
                continue;
            }

            var attr = match.Groups["attr"].Value.ToLowerInvariant();

            // Given a transform can have multiple types of definition we prioritise to use the matrix
            // definition over scale, translate and rotate. Technically it should have matrix or other types
            // but not both.
            switch (attr)
            {
                case "matrix":
                    {
                        var m = ParseMatrix(transformDefinition);

                        transform.Rotate += new GeometryRotate(GeometryUtils.RadiansToDegrees(m.GetRotation()), 0.0, 0.0);
                        transform.Scale *= m.GetScale();
                        transform.Translate += m.GetTranslation();
                    }
                    break;
                case "scale":
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
                    break;

                case "rotate":
                    {
                        var rotation = ParseRotate(transformDefinition);
                        transform.Rotate.Angle += rotation.Angle;
                    }
                    break;

                case "translate":
                    {
                        var translation = ParseTranslate(transformDefinition);
                        transform.Translate += translation;
                    }
                    break;

                case "skewx":
                    {
                        var skew = ParseSkewX(transformDefinition);
                        transform.SkewX *= skew;
                    }
                    break;
                case "skewy":
                    {
                        var skew = ParseSkewY(transformDefinition);
                        transform.SkewY *= skew;
                    }
                    break;
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
                if (double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
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
            .Select(x => double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : defaultValue)
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
}
