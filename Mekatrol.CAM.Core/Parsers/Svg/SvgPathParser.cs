using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;
using System.Text.RegularExpressions;

namespace Mekatrol.CAM.Core.Parsers.Svg;

internal class SvgPathParser(string data)
{
    private readonly string _data = data;
    private int _index = 0;
    private PointDouble? _startLocation;
    private PointDouble _currentLocation = new(0, 0);
    private readonly List<IGeometricEntity> _geometries = [];

    public (PointDouble startLocation, IList<IGeometricEntity> path, bool closed) Parse()
    {
        // Default to path not closed
        var pathIsClosed = false;

        // Reset locations
        _startLocation = null;
        _currentLocation = new PointDouble(0, 0);

        _geometries.Clear();

        SvgPathCommand? command;
        while ((command = GetCommand()) != null)
        {
            switch (command.Value)
            {
                // Move to
                case SvgPathCommand.m:
                case SvgPathCommand.M:
                    ParseMove(command.Value);

                    // Only set start location if not already set
                    if (_startLocation == null)
                    {
                        _startLocation = _currentLocation;
                    }
                    break;

                // Line to
                case SvgPathCommand.l:
                case SvgPathCommand.L:
                    _geometries.AddRange(ParseLine(command.Value));
                    break;

                case SvgPathCommand.h:
                case SvgPathCommand.H:
                    _geometries.Add(ParseHorizontalLine(command.Value));
                    break;

                case SvgPathCommand.v:
                case SvgPathCommand.V:
                    _geometries.Add(ParseVerticalLine(command.Value));
                    break;

                // Cubic bezier curve
                case SvgPathCommand.c:
                case SvgPathCommand.C:
                case SvgPathCommand.s:
                case SvgPathCommand.S:
                    _geometries.Add(ParseCubicBezier(command.Value));
                    break;

                // Quadratic bezier curve
                case SvgPathCommand.q:
                case SvgPathCommand.Q:
                case SvgPathCommand.t:
                case SvgPathCommand.T:
                    _geometries.Add(ParseQuadraticBezier(command.Value));
                    break;

                // Eliptical arc
                case SvgPathCommand.a:
                case SvgPathCommand.A:
                    _geometries.AddRange(ParseArc(command.Value));
                    break;

                // Close path
                case SvgPathCommand.z:
                case SvgPathCommand.Z:
                    pathIsClosed = true;
                    if (_startLocation != null)
                    {
                        // Close the path
                        _geometries.Add(new LineEntity(_currentLocation, _startLocation.Value, new Transform()));

                        // The path is closed, so we will start a new path
                        _startLocation = null;
                    }
                    break;

                default:
                    throw new Exception($"Invalid path command '{command}'");
            }
        }

        // If we didnt set the start location then default to the current location
        if (_startLocation == null)
        {
            _startLocation = _currentLocation;
        }

        return (_startLocation.Value, _geometries, pathIsClosed);
    }

    private IGeometricEntity ParseCubicBezier(SvgPathCommand command)
    {
        IList<PointDouble> points = [];

        PointDouble? pair;
        while ((pair = GetDoublePair()) != null)
        {
            points.Add(pair.Value);
        }

        var cubicFull = command == SvgPathCommand.c || command == SvgPathCommand.C;

        var expectedPointCount = cubicFull ? 3 : 2;

        // The point count should be a modulus of the expected point count
        if (points.Count % expectedPointCount != 0)
        {
            throw new Exception($"Cubic Bezier incorrect number of points, {points.Count} were provided for command '{command}'.");
        }

        if (command == SvgPathCommand.c || command == SvgPathCommand.s)
        {
            // If using relative coordinates then adjust to current location
            for (var i = 0; i < points.Count; i++)
            {
                points[i] += _currentLocation;
            }
        }

        // Cubic Bezier start location
        var startLocation = _currentLocation;
        var endLocation = cubicFull ? points[2] : points[1];

        // The first control point is assumed to be the reflection of the second control point
        // on the previous command relative to the current point.
        // (If there is no previous command or if the previous command was not an C, c, S or s,
        // assume the first control point is coincident with the current point.) (x2, y2) is the
        // second control point(i.e., the control point at the end of the curve)

        PointDouble? reflectedControlPoint = null;
        var previousGeometry = _geometries.LastOrDefault();
        if (previousGeometry != null && previousGeometry.Type == GeometricEntityType.CubicBezier)
        {
            var bezier = (CubicBezierEntity)previousGeometry;
            reflectedControlPoint = GeometryUtils.GetReflectedPoint(bezier.Control2, startLocation);
        }

        if (reflectedControlPoint == null)
        {
            reflectedControlPoint = _currentLocation;
        }

        // Depending on command type the first control point is not included
        var control1 = cubicFull ? points[0] : reflectedControlPoint ?? new PointDouble();
        var control2 = cubicFull ? points[1] : points[0];

        // Current location now end location
        _currentLocation = endLocation;

        // Pass start location, control 1, control 2, end location.
        return new CubicBezierEntity(startLocation, control1, control2, endLocation, new Transform());
    }

    private IGeometricEntity ParseQuadraticBezier(SvgPathCommand command)
    {
        IList<PointDouble> points = [];

        PointDouble? pair;
        while ((pair = GetDoublePair()) != null)
        {
            points.Add(pair.Value);
        }

        var quadtraticFull = command == SvgPathCommand.q || command == SvgPathCommand.Q;

        var expectedPointCount = quadtraticFull ? 2 : 1;

        // The point count should be a modulus of the expected point count
        if (points.Count % expectedPointCount != 0)
        {
            throw new Exception($"Quadratic Bezier incorrect number of points, {points.Count} were provided for command '{command}'.");
        }

        if (command == SvgPathCommand.q || command == SvgPathCommand.t)
        {
            // If using relative coordinates then adjust to current location
            for (var i = 0; i < points.Count; i++)
            {
                points[i] += _currentLocation;
            }
        }

        // Cubic Bezier start location
        var startLocation = _currentLocation;
        var endLocation = quadtraticFull ? points[1] : points[0];

        // The first control point is assumed to be the reflection of the second control point
        // on the previous command relative to the current point.
        // (If there is no previous command or if the previous command was not an Q, q, T or t,
        // assume the first control point is coincident with the current point.)

        PointDouble? reflectedControlPoint = null;
        var previousGeometry = _geometries.LastOrDefault();
        if (previousGeometry != null && previousGeometry.Type == GeometricEntityType.QuadraticBezier)
        {
            var bezier = (QuadraticBezier)previousGeometry;
            reflectedControlPoint = GeometryUtils.GetReflectedPoint(bezier.Control, startLocation);
        }

        if (reflectedControlPoint == null)
        {
            reflectedControlPoint = _currentLocation;
        }

        // Depending on command type the first control point is not included
        var control = quadtraticFull ? points[0] : reflectedControlPoint ?? new PointDouble();

        // Current location now end location
        _currentLocation = endLocation;

        return new QuadraticBezier(startLocation, control, endLocation, new Transform());
    }

    private void ParseMove(SvgPathCommand command)
    {
        // Some SVG have multiple positions in the move command so
        // use the last value in the list
        PointDouble? point;
        while ((point = GetDoublePair()) != null)
        {
            if (point == null)
            {
                throw new Exception($"{command} missing location value");
            }

            // Absolute move?
            if (command == SvgPathCommand.M)
            {
                _currentLocation = point.Value;
                continue;
            }

            // Relative move
            _currentLocation = new PointDouble(_currentLocation.X + point.Value.X, _currentLocation.Y + point.Value.Y);
        }
    }

    private IList<IGeometricEntity> ParseLine(SvgPathCommand command)
    {
        var lines = new List<IGeometricEntity>();

        // Loop until we dont find another double value
        while (_index < _data.Length)
        {
            var x = GetDouble();
            if (x == null)
            {
                // Double value not found, must be end of line value pairs
                break;
            }

            SkipWhitespaceAndComma();

            var y = GetDouble();
            y ??= 0;

            // If relative, add current location X,Y
            if (command == SvgPathCommand.l)
            {
                x += _currentLocation.X;
                y += _currentLocation.Y;
            }

            // Start at current location
            var startLocation = _currentLocation;

            // Update current location to line end location
            _currentLocation = new PointDouble(x.Value, y.Value);

            // Create the line from the start and end location
            lines.Add(new LineEntity(startLocation, _currentLocation, new Transform()));
        }

        return lines;
    }

    private IGeometricEntity ParseHorizontalLine(SvgPathCommand command)
    {
        var x = GetDouble();
        x ??= 0;

        // If relative, add current location X
        if (command == SvgPathCommand.h)
        {
            x += _currentLocation.X;
        }

        // Start at current location
        var startLocation = _currentLocation;

        // Update current location to line end location
        _currentLocation = new PointDouble(x.Value, startLocation.Y);

        // Create the line from the start and end location
        return new LineEntity(startLocation, _currentLocation, new Transform());
    }

    private IGeometricEntity ParseVerticalLine(SvgPathCommand command)
    {
        var y = GetDouble();
        y ??= 0;

        // If relative, add current location Y
        if (command == SvgPathCommand.v)
        {
            y += _currentLocation.Y;
        }

        // Start at current location
        var startLocation = _currentLocation;

        // Update current location to line end location
        _currentLocation = new PointDouble(startLocation.X, y.Value);

        // Create the line from the start and end location
        return new LineEntity(startLocation, _currentLocation, new Transform());
    }

    private IList<IGeometricEntity> ParseArc(SvgPathCommand command)
    {
        var geometries = new List<IGeometricEntity>();

        while (_index < _data.Length)
        {
            var startLocation = _currentLocation;

            var rx = GetDouble();

            // Stop reading arcs one no more double values
            if (rx == null)
            {
                break;
            }

            var ry = GetDouble() ?? 0;
            var angle = GetDouble() ?? 0;
            var isLargeArc = GetBoolean() ?? false;
            var isIncreasingSweep = GetBoolean() ?? false;
            var dx = GetDouble() ?? 0;
            var dy = GetDouble() ?? 0;

            // Update the current location to the end point of the arc
            _currentLocation = command == SvgPathCommand.a
                ? new PointDouble(startLocation.X + dx, startLocation.Y + dy)
                : new PointDouble(dx, dy);

            var geometry = GeometryUtils.GenerateArc(
                // Start point
                startLocation.X, startLocation.Y,

                // End point
                _currentLocation.X, _currentLocation.Y,

                // Radii
                rx.Value,
                ry,

                // Angle in degrees
                angle,

                // True if is a large arc
                isLargeArc,

                // True to sweep in clockwise (increasing angle) direction
                isIncreasingSweep);

            if (geometry != null)
            {
                geometries.Add(geometry);
            }
        }

        return geometries;
    }

    private bool? GetBoolean()
    {
        SkipWhitespaceAndComma();

        // Null when data finished
        if (_index >= _data.Length)
        {
            return null;
        }

        var booleanValue = _data[_index];

        if (booleanValue != '0' && booleanValue != '1')
        {
            return null;
        }

        _index++;

        // Return true if 1
        return booleanValue == '1';
    }

    private double? GetDouble()
    {
        SkipWhitespaceAndComma();

        // Null when data finished
        if (_index >= _data.Length)
        {
            return null;
        }

        var match = Regex.Match(_data[_index..], SvgParser.DoublePattern);

        // needs to be a match and the match needs to be at the start of the string
        if (!match.Success || match.Index != 0)
        {
            return null;
        }

        // If we matched the regex then we know it is a valid pair of values
        var v = double.Parse(match.Value.Trim());

        // Move past matched value
        _index += match.Length;

        return v;
    }

    private PointDouble? GetDoublePair()
    {
        SkipWhitespaceAndComma();

        // Null when data finished
        if (_index >= _data.Length)
        {
            return null;
        }

        var match = Regex.Match(_data[_index..], SvgParser.DoublePairPattern);

        // needs to be a match and the match needs to be at the start of the string
        if (!match.Success || match.Index != 0)
        {
            return null;
        }

        // If we matched the regex then we know it is a valid pair of values
        var (value1, value2) = GetSplitValue(match.Value.Trim());

        var x = double.Parse(value1);
        var y = double.Parse(value2);

        // Move past matched value
        _index += match.Length;

        return new PointDouble(x, y);
    }

    private SvgPathCommand? GetCommand()
    {
        SkipWhitespace();

        if (_index >= _data.Length)
        {
            return null;
        }

        var commandText = _data[_index];

        var command = commandText switch
        {
            'a' => SvgPathCommand.a,
            'A' => SvgPathCommand.A,
            'm' => SvgPathCommand.m,
            'M' => SvgPathCommand.M,
            'l' => SvgPathCommand.l,
            'L' => SvgPathCommand.L,
            'h' => SvgPathCommand.h,
            'H' => SvgPathCommand.H,
            'v' => SvgPathCommand.v,
            'V' => SvgPathCommand.V,
            'c' => SvgPathCommand.c,
            'C' => SvgPathCommand.C,
            's' => SvgPathCommand.s,
            'S' => SvgPathCommand.S,
            'q' => SvgPathCommand.q,
            'Q' => SvgPathCommand.Q,
            't' => SvgPathCommand.t,
            'T' => SvgPathCommand.T,
            'z' => SvgPathCommand.z,
            'Z' => SvgPathCommand.Z,
            _ => throw new Exception($"Invalid path command '{commandText}'"),
        };

        _index++;

        return command;
    }

    private void SkipWhitespaceAndComma()
    {
        SkipWhitespace();
        Skip(',');
        SkipWhitespace();
    }

    private void SkipWhitespace()
    {
        while (_index < _data.Length && char.IsWhiteSpace(_data[_index]))
        {
            _index++;
        }
    }

    private void Skip(char c)
    {
        if (_index < _data.Length && _data[_index] == c)
        {
            _index++;
        }
    }

    internal static (string value1, string value2) GetSplitValue(string all)
    {
        // A pair of points can take multiple forms where the value separator
        // can be one of , + -
        // The following are valid values:
        // +123,456
        // -123-456
        // +123+456
        // 123,-456
        // 123,+456
        // 123.054E-5+23

        var separators = new[] { ',', '+', '-', ' ' };
        var isExponent = false;
        var index = 0;
        var value1Text = string.Empty;
        while (index < all.Length)
        {
            // Is this an exponent chracter
            if (all[index] == 'E' || all[index] == 'e')
            {
                value1Text += all[index];
                isExponent = true;
                index++;
                continue;
            }

            // Did we find the separator, if so stop processing this string 
            if (!isExponent && index != 0 && separators.Contains(all[index]))
            {
                break;
            }

            // Add value to text                
            value1Text += all[index++];

            // have we finished getting the exponent
            if (!char.IsDigit(all[index]))
            {
                isExponent = false;
            }
        }

        if (all[index] == ',')
        {
            index++;
        }

        var value2Text = all[index..];
        return (value1Text.Trim(), value2Text.Trim());
    }
}
